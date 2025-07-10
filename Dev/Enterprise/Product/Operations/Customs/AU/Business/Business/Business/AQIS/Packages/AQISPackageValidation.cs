using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPackageValidation : ZValidation
	{
		public AQISPackageValidation(AQISPackage parent)
			: base(parent)
		{
			this.aQISPackage = parent;
		}

		public override void ValidateAll()
		{
			ValidateType();
			ValidateNumber();
		}

		public override Type AutoValidationType
		{
			get { return typeof(AQISPackageValidation); }
		}

		#region Number

		public void ValidateNumber()
		{
			ValidateCalculatedProperty(aQISPackage.NumberInfo);
		}

		protected void CheckNumber()
		{
			if (!aQISPackage.IsValidationSuspended)
			{
				if (aQISPackage.Number < 0)
				{
					aQISPackage.NumberInfo.AddError("Cannot enter a negative Package Number");
				}

				if (aQISPackage.Number == 0 && !aQISPackage.Type.IsEmpty)
				{
					aQISPackage.NumberInfo.AddError("Please enter a Package Number");
				}

				ValidateOnly10Records();
			}
		}

		#endregion

		#region Type

		public void ValidateType()
		{
			ValidateCalculatedProperty(aQISPackage.TypeInfo);
		}

		protected void CheckType()
		{
			if (!aQISPackage.IsValidationSuspended)
			{
				ListValidation.MessageErrorIfInvalidCode(aQISPackage.TypeInfo, aQISPackage.Lookups.AQISPackageTypeList);

				if (!aQISPackage.Type.IsEmpty)
				{
					if (aQISPackage.ParentCollections.Count > 0)
					{
						AQISPackageCollection pacakges = ((AQISPackageCollection)aQISPackage.ParentCollections.First());

						foreach (AQISPackage currentPacakge in pacakges)
						{
							if (currentPacakge.Type != "" && currentPacakge != aQISPackage && currentPacakge.Type == aQISPackage.Type)
							{
								aQISPackage.TypeInfo.AddError("You cannot repeat Package Types. Please select different Package Types.");
								break;
							}
						}
					}

					if (aQISPackage.Type.Contains('/') || aQISPackage.Type.Contains(','))
					{
						aQISPackage.TypeInfo.AddError("You cannot use the characters '/' or ','");
					}
				}

				if (aQISPackage.Type.IsEmpty && aQISPackage.Number > 0)
				{
					aQISPackage.TypeInfo.AddError("Please enter a Package Type");
				}

				ValidateOnly10Records();
			}
		}

		#endregion

		void ValidateOnly10Records()
		{
			if (aQISPackage.ParentCollections.Count > 0)
			{
				aQISPackage.ClearRowNotifications();

				AQISPackageCollection packages = ((AQISPackageCollection)aQISPackage.ParentCollections.First());

				if (packages != null && packages.Count > 10 && (!packages[10].Type.IsEmpty || !packages[10].Number.IsEmpty))
				{
					aQISPackage.AddRowError("You can only enter 10 AQIS Packages");
				}
			}
		}

		readonly AQISPackage aQISPackage;
	}
}
