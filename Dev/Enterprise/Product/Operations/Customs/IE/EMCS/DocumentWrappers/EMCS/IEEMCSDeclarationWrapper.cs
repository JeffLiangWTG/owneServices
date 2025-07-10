using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.DocumentWrappers.Customs.EU.EMCS;

namespace Enterprise.Customs.IE.EMCS.DocumentWrappers
{
	public class IEEMCSDeclarationWrapper : EMCSDeclarationWrapper
	{
		public IEEMCSDeclarationWrapper(EMCSJobDeclaration declaration, BusinessObjectFactory factory) : base(declaration, factory)
		{
		}

		public static IEEMCSDeclarationWrapper New(EMCSJobDeclaration declaration, BusinessObjectFactory factory)
		{
			return new IEEMCSDeclarationWrapper(declaration, factory);
		}

		protected override ZString Box24AProducedInUKLabelCore => string.Empty;
		protected override ZString Box24AProducedInUKCore => string.Empty;
		protected override ZString Box24BProducedInUKLabelCore => string.Empty;
		protected override ZString Box24BProducedInUKCore => string.Empty;
		protected override ZString Box24CProducedInUKLabelCore => string.Empty;
		protected override ZString Box24CProducedInUKCore => string.Empty;

		protected override ZString Box24ASoldInWarehouseLabelCore => string.Empty;
		protected override ZString Box24ASoldInWarehouseCore => string.Empty;
		protected override ZString Box24BSoldInWarehouseLabelCore => string.Empty;
		protected override ZString Box24BSoldInWarehouseCore => string.Empty;
		protected override ZString Box24CSoldInWarehouseLabelCore => string.Empty;
		protected override ZString Box24CSoldInWarehouseCore => string.Empty;

		protected override ZString BoxZAdministrativeReferenceCodeCore => base.BoxZAdministrativeReferenceCodeCore.IfEmptyUse(() => FallbackString);

		static ZString FallbackString => Res.GetString("1C24C14F-5B15-4FD0-B368-D3713F504BA6", "FALLBACK");
	}
}
