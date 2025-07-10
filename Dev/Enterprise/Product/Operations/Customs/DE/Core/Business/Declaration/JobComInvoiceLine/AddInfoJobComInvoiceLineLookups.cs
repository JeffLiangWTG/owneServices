using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent.Parent;

		public CodeDescriptionPairList CessionFlagList
		{
			get
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine.Declaration is JobDeclaration declaration)
				{
					var formattedProcedure = invoiceLine.JI_FormattedProcedure;
					var shipmentType = declaration.JE_MessageType;

					return Factory.GetCachedValue($"CessionFlagList|{formattedProcedure}|{shipmentType}", () =>
					{
						var result = new CodeDescriptionPairList();
						var procedureCode = formattedProcedure.Left(2);
						var previousProcedureCode = formattedProcedure.SubstringSafe(2, 2);
						var concession = formattedProcedure.SubstringSafe(4, 3);
						if (!procedureCode.IsEmpty && !previousProcedureCode.IsEmpty && !concession.IsEmpty)
						{
							var flagList = RefCusProcedureAttribute.Loader.LoadAttributesList(Factory, procedureCode, previousProcedureCode, concession, Core.Constants.CountryCodes.Germany, shipmentType, AttributeNames.Codes.TAXFLAG);
							var fullCessionFlagList = new CessionFlagList();
							foreach (var flag in flagList)
							{
								result.AddPair(flag, fullCessionFlagList.GetDescriptionFromCode(flag));
							}
						}
						return result;
					});
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public CodeDescriptionPairList CustomsUQList => InvoiceLine.Lookups.CustomsUQList;

		public CodeDescriptionPairList EconomicConditionsList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_A2055, ZDateTime.Today);

		public CodeDescriptionPairList IdentificationMeansTypeList => Factory.GetCachedValue<IdentificationMeansTypeList>();
	}
}
