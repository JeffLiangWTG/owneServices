using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class FinalPriceReportByDateExtensionLineCollection : NonPersistentBusinessObjectCollection<FinalPriceReportByDateExtensionLine>, IEmbeddedModulePopupCollection
	{
		public FinalPriceReportByDateExtensionLineCollection(FinalPriceReportByDateExtensionHeader parent) : base(parent.Factory)
		{
			Argument.NotNull(parent, nameof(parent));
			Parent = parent;
		}
		public FinalPriceReportByDateExtensionHeader Parent { get; }
		protected override BusinessObject CreateNonPersistentBusinessObject() => new FinalPriceReportByDateExtensionLine(Parent);

		public FinalPriceReportByDateExtensionLine AddNewLine(KREntryHeaderDetailsView moduleBO)
		{
			var line = AddNew();
			line.ImportDeclarationNumber = moduleBO.KEH_EntryNum.Left(AutoFinalPriceReportByDateExtensionLine.Schema.ImportDeclarationNumberMaxLength);
			return line;
		}
		BusinessObject IEmbeddedModulePopupCollection.AddNewLine(BusinessObject moduleBO) => AddNewLine((KREntryHeaderDetailsView)moduleBO);
	}
}
