using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclarationValidation : AutoIEJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent) : base(parent) { }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateInvoiceLinesForOverlappingPackSets();
		}

		void ValidateInvoiceLinesForOverlappingPackSets()
		{
			var declaration = Parent;
			declaration.OverlappingPackageInvoiceLines = new List<ZGuid>();
			var pivots = declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.PackagesPivot).Cast<InvoiceLinePackagePivot>();
			var overLappingPivotsGroups = pivots.GroupBy(x => x.CHC_CW).Where(x => x.Count() > 1);
			foreach (var overLappingPivotsGroup in overLappingPivotsGroups)
			{
				var firstPivot = overLappingPivotsGroup.FirstOrDefault();
				var otherPivots = overLappingPivotsGroup.Where(x => x != firstPivot);
				var firstPivotInvoiceLinePackages = firstPivot.InvoiceLine.PackagesPivot.Select(x => x.CHC_CW).ToHashSet();
				if (otherPivots.Any(pivot => !pivot.InvoiceLine.PackagesPivot.Select(x => x.CHC_CW).ToHashSet().SetEquals(firstPivotInvoiceLinePackages)))
				{
					declaration.OverlappingPackageInvoiceLines.AddRange(overLappingPivotsGroup.Select(pivot => pivot.CHC_JI));
				}
			}
		}

		protected override void CheckJE_UCR()
		{
			if (Parent.IsMiscellaneous)
			{
				base.CheckJE_UCR();
			}
		}

		protected override void CheckJE_TransportMeans()
		{
			if (Parent.JE_TransportMeans.IsEmpty && IsJE_TransportMeansRequired)
			{
				var info = Parent.JE_TransportMeansInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
		}

		protected virtual bool IsJE_TransportMeansRequired => Parent.IsJE_TransportMeansRequired;

		protected override void CheckJE_RN_NKTransportNationality()
		{
			base.CheckJE_RN_NKTransportNationality();
			if (Parent.IsTransportNationalityMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			base.CheckJE_LocationOfGoods();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo);
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override void CheckJE_PaymentMethodLogicForEU()
		{
			//Confirmed with BP to be done in later WI.
		}

		protected override void CheckJE_LocationQualifier()
		{
			base.CheckJE_LocationQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationQualifierInfo);
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			base.CheckJE_LocationOtherInformation();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOtherInformationInfo);
		}

		#region No-Amend check

		protected void CheckNoAmendingOnEntryStatus(IZType originalValue, ZPropertyInfo targetInfo) => CheckNoAmendingOnEntryStatus(
			() => !originalValue.IsEmpty && !originalValue.Equals(targetInfo.Value),
			targetInfo
		);

		protected void CheckNoAmendingOnEntryStatus(Func<bool> valueChanged, ZPropertyInfo targetInfo)
		{
			if (Parent.IsAmendmentValidationMode && valueChanged())
			{
				targetInfo.AddMessageError(CommonResStrings.ShouldNotAmendThisValue);
			}
		}
		#endregion No-Amend check
	}
}
