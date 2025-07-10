using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackageValidation : Customs.Business.CusDecHouseContainerPackValidation
	{
		public PackageValidation(Package parent)
			: base(parent)
		{
		}

		public new Package Parent
		{
			get { return (Package)base.Parent; }
		}

		public JobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		protected override void CheckCW_MarksAndNos()
		{
			base.CheckCW_MarksAndNos();
			if (Parent.Declaration != null && Parent.Declaration.IsPost)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CW_MarksAndNosInfo, "No Marks and Numbers are Entered. For Transport Mode Post, Marks and Numbers are Mandatory. Please enter the Marks and Numbers");
			}
		}

		protected override void CheckCW_OuterPacks()
		{
			base.CheckCW_OuterPacks();

			if (Declaration != null && Declaration.IsAir && !Parent.CW_OuterPacks.IsEmpty)
			{
				Parent.CW_OuterPacksInfo.AddMessageError(OuterPacksIsNotAllowedOnAir);
			}
			else if (Parent.CW_OuterPacks > CW_OuterPacksMaxValue)
			{
				Parent.CW_OuterPacksInfo.AddMessageError(OuterPacksIsTooLong);
			}
			else if (Parent.CW_OuterPacks > Parent.CW_PackQty)
			{
				Parent.CW_OuterPacksInfo.AddMessageError(OuterPacksIsGreaterThanPackQty);
			}
		}

		protected override void CheckCW_InBondPackQty()
		{
			base.CheckCW_InBondPackQty();

			if (Parent.CW_InBondPackQty > 0 && Declaration != null && !Declaration.HasNature10And20Entries && !Declaration.HasNature20Entry)
			{
				Parent.CW_InBondPackQtyInfo.AddMessageError("Warehouse Number of Packages should not be entered if the Declaration is not Nature 10/20 or Nature 20.");
			}
			else if (Parent.PackingGroup != null && Parent.PackingGroup.WarehouseNumberOfPackages == 0 && Declaration != null && (Declaration.HasNature10And20Entries || Declaration.HasNature20Entry))
			{
				Parent.CW_InBondPackQtyInfo.AddWarning("You have not entered the Warehouse Number of Packages.");
			}
		}

		protected override void CheckCW_PackQty()
		{
			base.CheckCW_PackQty();

			if (Declaration != null)
			{
				if (Parent.PackingGroup != null)
				{
					var totalPackages = Parent.PackingGroup.TotalNumberOfPackages;
					if (totalPackages == 0)
					{
						Parent.CW_PackQtyInfo.AddMessageError("You have not entered the Total Number of Packages.");
					}
					else if (Declaration.HasNature10And20Entries && totalPackages < Parent.PackingGroup.WarehouseNumberOfPackages)
					{
						Parent.CW_PackQtyInfo.AddWarning("Total Number of Packages should be greater than or equal to Warehouse Number of Packages for Nature 10/20 Entries.");
					}
					else if (Declaration.HasNature20Entry && !Declaration.HasNature10And20Entries && totalPackages != Parent.PackingGroup.WarehouseNumberOfPackages)
					{
						Parent.CW_PackQtyInfo.AddWarning("Total Number of Packages and Warehouse Number of Packages should be the same for Nature 20 Entries.");
					}
					else if (totalPackages == int.MaxValue)
					{
						Parent.CW_PackQtyInfo.AddMessageError("The total number of packages exceeds the maximum allowed amount. Check packing lines for data entry errors.");
					}
				}

				Declaration.Validation.ValidateJE_TotalNoOfPacks();
			}
		}

		const int CW_OuterPacksMaxValue = 9999;

		public const string OuterPacksIsNotAllowedOnAir = "You cannot enter the Packing Unit Count when transport mode is 'Air'.";
		public const string OuterPacksIsTooLong = "The Outer Packing Unit Count cannot be greater than 9999.";
		public const string OuterPacksIsGreaterThanPackQty = "The Outer Packing Unit Count cannot be greater than the Total Number of Packages.";
	}
}
