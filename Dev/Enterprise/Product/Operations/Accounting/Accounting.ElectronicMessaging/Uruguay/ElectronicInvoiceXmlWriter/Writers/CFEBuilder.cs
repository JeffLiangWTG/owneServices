using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.MasterFiles.Business.CountryCompliance.UruguayComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface ICFEBuilder
	{
		CFEDefType BuildCFEInfo(TransactionInfo transaction);
	}

	class CFEBuilder : ICFEBuilder
	{
		public CFEBuilder()
		{
			factory_InitializedOnly = new BusinessObjectFactory();
			encabezadoBuilder_constructorInitializedOnly = new EncabezadoBuilder();
			itemsBuilder_constructorInitializedOnly = new ItemsBuilder();
			referenciaBuilder_constructorInitializedOnly = new ReferenciaBuilder();
		}

		CFEDefType ICFEBuilder.BuildCFEInfo(TransactionInfo transaction)
		{
			var oCFE = new CFEDefType();

			switch (transaction?.ComplianceSubType)
			{
				case ComplianceSubTypeCodes.TXI:
				case ComplianceSubTypeCodes.TCR:
				case ComplianceSubTypeCodes.TCD:
				case ComplianceSubTypeCodes.YXI:
				case ComplianceSubTypeCodes.YCR:
				case ComplianceSubTypeCodes.YCD:
					oCFE.Item = BuildEFactura(transaction);
					break;
				case ComplianceSubTypeCodes.TKT:
				case ComplianceSubTypeCodes.TKC:
				case ComplianceSubTypeCodes.TKD:
				case ComplianceSubTypeCodes.YKT:
				case ComplianceSubTypeCodes.YKR:
				case ComplianceSubTypeCodes.YKD:
					oCFE.Item = BuildETicket(transaction);
					break;
			}

			return oCFE;
		}

		CFEDefTypeEFact BuildEFactura(TransactionInfo transaction)
		{
			var eFac = new CFEDefTypeEFact()
			{
				Detalle = ItemsBuilder.BuildItems(transaction),
				Referencia = ReferenciaBuilder.BuildReferenciaInfo(transaction)
			};

			eFac.Encabezado = EncabezadoBuilder.BuildEFacEncabezadoInfo(transaction, eFac.Detalle, Factory);

			return eFac;
		}

		CFEDefTypeETck BuildETicket(TransactionInfo transaction)
		{
			var eTick = new CFEDefTypeETck()
			{
				Detalle = ItemsBuilder.BuildItems(transaction),
				Referencia = ReferenciaBuilder.BuildReferenciaInfo(transaction)
			};

			eTick.Encabezado = EncabezadoBuilder.BuildETicketEncabezadoInfo(transaction, eTick.Detalle, Factory);

			return eTick;
		}

		IEncabezadoBuilder EncabezadoBuilder => encabezadoBuilder_constructorInitializedOnly;
		IEncabezadoBuilder encabezadoBuilder_constructorInitializedOnly;
		IItemsBuilder ItemsBuilder => itemsBuilder_constructorInitializedOnly;
		IItemsBuilder itemsBuilder_constructorInitializedOnly;
		IReferenciaBuilder ReferenciaBuilder => referenciaBuilder_constructorInitializedOnly;
		IReferenciaBuilder referenciaBuilder_constructorInitializedOnly;
		BusinessObjectFactory Factory => factory_InitializedOnly;
		BusinessObjectFactory factory_InitializedOnly;

#if DEBUG
		public void SubstituteEncabezadoBuilder_ForTestOnly(IEncabezadoBuilder replacement) => encabezadoBuilder_constructorInitializedOnly = replacement;
		public IEncabezadoBuilder EncabezadoBuilder_ExposedForTestOnly => EncabezadoBuilder;
		public void SubstituteItemsBuilder_ForTestOnly(IItemsBuilder replacement) => itemsBuilder_constructorInitializedOnly = replacement;
		public IItemsBuilder iTemsBuilder_constructorInitializedOnly => ItemsBuilder;
		public void SubstituteReferenciaBuilder_ForTestOnly(IReferenciaBuilder replacement) => referenciaBuilder_constructorInitializedOnly = replacement;
		public IReferenciaBuilder ReferenciaBuilder_ExposedForTestOnly => ReferenciaBuilder;
		public void SubstituteFactory_ForTestOnly(BusinessObjectFactory replacement) => factory_InitializedOnly = replacement;
#endif
	}
}
