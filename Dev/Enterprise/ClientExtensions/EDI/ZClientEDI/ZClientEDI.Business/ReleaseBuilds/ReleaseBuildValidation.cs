//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoReleaseBuildValidation
//
//    This class should be used for overriding validation in AutoReleaseBuildValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business
{
	public class ReleaseBuildValidation : AutoReleaseBuildValidation
	{
		public ReleaseBuildValidation(AutoReleaseBuild parent)
			: base(parent)
		{
		}

		ReleaseBuild ParentReleaseBuild
		{
			get { return (ReleaseBuild)Parent; }
		}

		protected override void CheckHL_ReleaseStatus()
		{
			if (ParentReleaseBuild.IsCargoWiseProduct)
			{
				base.CheckHL_ReleaseStatus();
				MandatoryValidation.CheckEntered(Parent.HL_ReleaseStatusInfo, "Status");
				ListValidation.ErrorIfInvalidCode(Parent.HL_ReleaseStatusInfo, Parent.Lookups.Statuses);
			}
		}

		protected override void CheckHL_Product()
		{
			base.CheckHL_Product();
			MandatoryValidation.CheckEntered(Parent.HL_ProductInfo);
			if (!ParentReleaseBuild.IsCargoWiseProduct)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HL_ProductInfo);
			}
		}

		protected override void CheckHL_ExeVersionDate()
		{
			base.CheckHL_ExeVersionDate();
			MandatoryValidation.CheckEntered(Parent.HL_ExeVersionDateInfo);
		}

		protected override void CheckHL_Superceded()
		{
			base.CheckHL_Superceded();
			if (!Parent.HL_Superceded && ParentReleaseBuild.IsCargoWiseProduct)
			{
				ZQuery filter = new ZQuery(ReleaseBuildSchema.HL_ReleaseStatus, Parent.HL_ReleaseStatus);
				filter.AddToFilter(ReleaseBuildSchema.HL_Superceded, ZBool.False);
				filter.AddToFilter(ReleaseBuildSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var collection = Parent.Factory.Load<ReleaseBuild>(filter).OrderBy(c => c.VersionNumber).ToArray();

				if (collection.Length > 1)
				{
					Parent.HL_SupercededInfo.AddError("There are already two other non-superseded builds (" + collection[0].ExeVersion + " & " + collection[1].ExeVersion + ") on the same release ring. Please mark one of these builds as superseded first if you wish to make this a new current build.");
				}
			}
		}

		public void ValidateExeVersion()
		{
			ValidateCalculatedProperty(ParentReleaseBuild.ExeVersionInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044")]
		protected void CheckExeVersion()
		{
			if (!System.Text.RegularExpressions.Regex.IsMatch(ParentReleaseBuild.ExeVersion, @"^[0-9]+.[0-9]+.[0-9]+.[0-9]+$") || ParentReleaseBuild.ExeVersion == "0.0.0.0")
			{
				ParentReleaseBuild.ExeVersionInfo.AddError("Please ensure format is 'MajorVersion'.'MinorVersion'.'Release'.'Product'");
			}
			else if (!ParentReleaseBuild.ExeVersion.IsEmpty && !Parent.HL_Product.IsEmpty)
			{
				var versionNumber = ReleaseBuild.GetVersionNumberFromText(ParentReleaseBuild.ExeVersion);
				ZQuery filter = new ZQuery(ReleaseBuildSchema.HL_Product, SQLComparisonOperator.Equal, Parent.HL_Product);
				filter.AddToFilter(ReleaseBuildSchema.HL_MajorVersion, SQLComparisonOperator.Equal, versionNumber.Major);
				filter.AddToFilter(ReleaseBuildSchema.HL_MinorVersion, SQLComparisonOperator.Equal, versionNumber.Minor);
				filter.AddToFilter(ReleaseBuildSchema.HL_Release, SQLComparisonOperator.Equal, versionNumber.Release);
				filter.AddToFilter(ReleaseBuildSchema.HL_Patch, SQLComparisonOperator.Equal, versionNumber.Patch);
				filter.AddToFilter(ReleaseBuildSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				ReleaseBuildCollection collection = new ReleaseBuildCollection(Parent.Factory, filter);
				collection.Load();

				if (collection.Count > 0)
				{
					ParentReleaseBuild.ExeVersionInfo.AddError("This Version number has already been used for the same product.");
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateExeVersion();
		}
	}
}

