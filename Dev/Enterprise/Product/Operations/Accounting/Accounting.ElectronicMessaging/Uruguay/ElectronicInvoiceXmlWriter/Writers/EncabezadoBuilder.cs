using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface IEncabezadoBuilder
	{
		CFEDefTypeEFactEncabezado BuildEFacEncabezadoInfo(TransactionInfo transaction, Item_Det_Fact[] detalle, BusinessObjectFactory factory);
		CFEDefTypeETckEncabezado BuildETicketEncabezadoInfo(TransactionInfo transaction, Item_Det_Fact[] detalle, BusinessObjectFactory factory);
	}

	class EncabezadoBuilder : IEncabezadoBuilder
	{
		public EncabezadoBuilder()
		{
			emisorBuilder_constructorInitializedOnly = new EmisorBuilder();
			idDocumentoBuilder_constructorInitializedOnly = new IdDocumentoBuilder();
			receptorBuilder_constructorInitializedOnly = new ReceptorBuilder();
			totalesBuilder_constructorInitializedOnly = new TotalesBuilder();
		}

		CFEDefTypeEFactEncabezado IEncabezadoBuilder.BuildEFacEncabezadoInfo(TransactionInfo transaction, Item_Det_Fact[] detalle, BusinessObjectFactory factory)
		{
			return new CFEDefTypeEFactEncabezado()
			{
				IdDoc = IdDocumentoBuilder.BuildEFacIdDocumento(transaction),
				Emisor = EmisorBuilder.BuildEmisorInfo(transaction, factory),
				Receptor = ReceptorBuilder.BuildEFacReceptor(transaction),
				Totales = TotalesBuilder.BuildTotales(transaction, detalle)
			};
		}

		CFEDefTypeETckEncabezado IEncabezadoBuilder.BuildETicketEncabezadoInfo(TransactionInfo transaction, Item_Det_Fact[] detalle, BusinessObjectFactory factory)
		{
			return new CFEDefTypeETckEncabezado()
			{
				IdDoc = IdDocumentoBuilder.BuildETicketIdDocumento(transaction),
				Emisor = EmisorBuilder.BuildEmisorInfo(transaction, factory),
				Receptor = ReceptorBuilder.BuildETicketReceptor(transaction),
				Totales = TotalesBuilder.BuildTotales(transaction, detalle)
			};
		}

		IEmisorBuilder EmisorBuilder => emisorBuilder_constructorInitializedOnly;
		IEmisorBuilder emisorBuilder_constructorInitializedOnly;
		IIdDocumentoBuilder IdDocumentoBuilder => idDocumentoBuilder_constructorInitializedOnly;
		IIdDocumentoBuilder idDocumentoBuilder_constructorInitializedOnly;
		IReceptorBuilder ReceptorBuilder => receptorBuilder_constructorInitializedOnly;
		IReceptorBuilder receptorBuilder_constructorInitializedOnly;
		ITotalesBuilder TotalesBuilder => totalesBuilder_constructorInitializedOnly;
		ITotalesBuilder totalesBuilder_constructorInitializedOnly;

#if DEBUG
		public void SubstituteEmisorBuilder_ForTestOnly(IEmisorBuilder replacement) => emisorBuilder_constructorInitializedOnly = replacement;
		public IEmisorBuilder EmisorBuilder_ExposedForTestOnly => EmisorBuilder;
		public void SubstituteIdDocumentoBuilder_ForTestOnly(IIdDocumentoBuilder replacement) => idDocumentoBuilder_constructorInitializedOnly = replacement;
		public IIdDocumentoBuilder IdDocumentoBuilder_ExposedForTestOnly => IdDocumentoBuilder;
		public void SubstituteReceptorBuilder_ForTestOnly(IReceptorBuilder replacement) => receptorBuilder_constructorInitializedOnly = replacement;
		public IReceptorBuilder ReceptorBuilder_ExposedForTestOnly => ReceptorBuilder;
		public void SubstituteTotalesBuilder_ForTestOnly(ITotalesBuilder replacement) => totalesBuilder_constructorInitializedOnly = replacement;
		public ITotalesBuilder TotalesBuilder_ExposedForTestOnly => TotalesBuilder;
#endif

	}
}
