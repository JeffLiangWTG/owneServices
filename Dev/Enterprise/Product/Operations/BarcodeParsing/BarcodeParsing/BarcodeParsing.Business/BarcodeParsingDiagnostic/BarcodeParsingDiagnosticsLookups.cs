using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeParsingDiagnosticsLookups : ZLookups
	{
		#region Constructors

		public BarcodeParsingDiagnosticsLookups(BarcodeParsingDiagnostics parent)
			: base(parent)
		{
		}

		new BarcodeParsingDiagnostics Parent => (BarcodeParsingDiagnostics)base.Parent;

		#endregion

		#region Properties

		public OrgHeaderCollection Buyers => BarcodeParsingLookupHelper.GetBuyers(Factory, Parent.ModuleCode);

		public OrgHeaderCollection Suppliers => BarcodeParsingLookupHelper.GetSuppliers(Factory, Parent.ModuleCode);

		public BarcodeModuleTypes ModuleTypes => BarcodeParsingLookupHelper.ModuleTypes(Factory);

		public IBusinessObjectCollection RelatedEntityList => BarcodeParsingLookupHelper.RelatedEntityList(Factory, Parent.ModuleCode, Parent.Buyer, Parent.Supplier);

		public ReadOnlyCodeDescriptionPairList DiagnosticsTypes => BarcodeParsingLookupHelper.DiagnosticsTypes(Factory);

		public ReadOnlyCodeDescriptionPairList TargetFields => BarcodeParsingLookupHelper.GetTargetFields(Factory, Parent.ModuleCode);

		#endregion
	}
}
