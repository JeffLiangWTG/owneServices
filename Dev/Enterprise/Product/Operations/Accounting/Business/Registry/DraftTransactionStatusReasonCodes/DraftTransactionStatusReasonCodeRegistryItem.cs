using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public sealed class DraftTransactionStatusReasonCodeRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<DraftTransactionStatusReasonCodeCollection, DraftTransactionStatusReasonCodeCollection>
	{
		public DraftTransactionStatusReasonCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, GetDataType(), storage, options, GetDefaultValue()))
		{
		}

		static DraftTransactionStatusReasonCodeRegistryDataType GetDataType()
		{
			return new DraftTransactionStatusReasonCodeRegistryDataType();
		}

		static DraftTransactionStatusReasonCodeCollection GetDefaultValue()
		{
			var defaultValue = new DraftTransactionStatusReasonCodeCollection();
			var defaultRow = new DraftTransactionStatusReasonCode();
			defaultRow.Code = DraftTransactionStatusReasonCode.DefaultReasonCode;
			defaultRow.ANL = true;
			defaultRow.DFT = true;
			defaultRow.DSC = true;
			defaultRow.DIS = true;
			defaultRow.AFP = true;
			defaultRow.AWA = true;
			defaultRow.PRS = true;
			defaultValue.Add(defaultRow);

			var chqRow = new DraftTransactionStatusReasonCode();
			chqRow.Code = "CHQ";
			chqRow.Description = ResString.GetMultilingualString("04363126-276D-4D09-87CA-B6F3DE2019F8", "Charge billed is higher than quoted. Querying with supplier.");
			chqRow.DIS = true;
			defaultValue.Add(chqRow);

			var njrRow = new DraftTransactionStatusReasonCode();
			njrRow.Code = "NJR";
			njrRow.Description = ResString.GetMultilingualString("F33AF34D-C681-4C4B-995B-6423DF253C1D", "No jobs or job references identified");
			njrRow.DFT = true;
			njrRow.AWA = true;
			njrRow.DIS = true;
			defaultValue.Add(njrRow);

			var nafRow = new DraftTransactionStatusReasonCode();
			nafRow.Code = "NAF";
			nafRow.Description = ResString.GetMultilingualString("79ABB646-528A-4778-BDFF-05359746DC9E", "No accruals found for listed jobs");
			nafRow.DFT = true;
			nafRow.AWA = true;
			nafRow.DIS = true;
			defaultValue.Add(nafRow);

			return defaultValue;
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.DraftTransactionStatusReasonCodeRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class DraftTransactionStatusReasonCodeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DraftTransactionStatusReasonCodeCollection>
	{
	}
}
