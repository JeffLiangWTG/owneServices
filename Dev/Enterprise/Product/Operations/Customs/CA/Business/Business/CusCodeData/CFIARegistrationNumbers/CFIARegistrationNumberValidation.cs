using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CFIARegistrationNumberValidation : CusCodeDataValidation
	{
		public CFIARegistrationNumberValidation(CFIARegistrationNumber parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			if (!ParentIsOkaCFIA)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
				if (!Parent.CY_Code.IsEmpty)
				{
					var query = new ZQuery(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CFIANumber);
					query.AddToFilter(CusCodeDataSchema.CY_Code, Parent.CY_Code);
					query.AddToFilter(CusCodeDataSchema.CY_ParentID, Parent.CY_ParentID);
					query.AddToFilter(CusCodeDataSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					if (Parent.Factory.Load<CFIARegistrationNumber>(query).Length > 0)
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("aba3c008-cb21-460f-af67-4c472fee0e7e", "This code is duplicated. Only one occurrence of each document number type is allowed."));
					}
				}
			}
		}

		protected override void CheckCY_Data()
		{
			if (!ParentIsOkaCFIA)
			{
				base.CheckCY_Data();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo, Res.GetString("d8508089-ba04-4c2f-8ff5-c8f15c7fd0ff", "Number"));
				RegistrationNumberHelper.ValidateCFIARegNum(Parent);
			}
		}

		protected new CFIARegistrationNumber Parent => (CFIARegistrationNumber)base.Parent;

		ZBool ParentIsOkaCFIA
		{
			get
			{
				if (parentIsOkaCFIA == null)
				{
					parentIsOkaCFIA = new CachedProperty<ZBool>(Parent.Factory, () =>
					{
						var line = Parent.Parent as JobComInvoiceLine;
						return line != null && (line.Declaration?.IsOGD ?? false) && line.CA_OGDStatus == AVSStatusList.Codes.WillBeApproved;
					});
				}
				return parentIsOkaCFIA.Value;
			}
		}
		CachedProperty<ZBool> parentIsOkaCFIA;
	}
}
