using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public abstract class ImportExportAwareSupportingInfo : Customs.Business.CusSupportingInfo
	{
		protected ImportExportAwareSupportingInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ICanBeImportOrExport ImportExportParent => Parent as ICanBeImportOrExport;

		public ZBool ParentIsJobComInvoiceLine => CSI_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix;

		public ZBool ParentIsJobComInvoiceHeader => CSI_ParentTableCode == JobComInvoiceHeaderSchema.Constants.Prefix;

		public ZBool IsParentInvoiceOrLineOrCusEntry => ParentIsJobComInvoiceHeader || ParentIsJobComInvoiceLine || CSI_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix;

		public bool IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod => IsParentInvoiceOrLineOrCusEntry && (Declaration?.IsTransitionPeriodAES30 ?? false);

		public JobDeclaration Declaration
		{
			get
			{
				JobDeclaration result = null;
				switch (Parent)
				{
					case JobDeclaration declaration:
						result = declaration;
						break;
					case JobComInvoiceHeader invoice:
						result = invoice.JobDeclaration;
						break;
					case JobComInvoiceLine invoiceLine:
						result = invoiceLine.Declaration;
						break;
					case CusEntryInstruction instruction:
						result = instruction.JobDeclaration;
						break;
				}
				return result;
			}
		}

		public bool IsCopyingExposed
		{
			get { return base.IsCopying; }
		}

		public abstract ZString KeyToDeterimeUniqueness { get; }
	}
}
