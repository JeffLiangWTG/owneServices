namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class AsycudaCustoms
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusClassification : IBaseCusClassification { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusContainer : Shared.IBaseCusContainer { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IBill : Customs.IBill { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IGroupInvoiceCharge { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IInvoiceCharge { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IInvoiceApportionCharge { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IInvoiceLineCharge { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : Shared.IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : Shared.IBaseJobComInvoiceHeader { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IOrgSupplierPart { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs country or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IAsycudaCustomsRegistry { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusInBondMoveHeader : Customs.ICusInBondMoveHeader { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusInBondHeader : Customs.ICusInBondHeader { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusGuaranteeHeader : IBaseCusGuaranteeHeader { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusVehicle : Customs.ICusVehicle { }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface IAsycudaDeclarationTypesProvider { }
		}
	}
}
