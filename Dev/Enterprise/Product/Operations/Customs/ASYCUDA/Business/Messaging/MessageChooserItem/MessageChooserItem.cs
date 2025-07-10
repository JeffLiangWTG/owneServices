using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class MessageChooserItem : AutoMessageChooserItem
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MessageChooserItem(MessageChooser chooser, ISelectionItem item, bool showStatus)
		{
			Chooser = Argument.NotNull(chooser, "chooser");
			BizO = item;
			this.showStatus = showStatus;
		}

		public override ZBool Checked
		{
			get => base.Checked;
			set
			{
				var old = Checked;
				base.Checked = value;
				if (old != Checked)
				{
					Chooser.SelectedDescriptionInfo.RefreshBinding();
				}
			}
		}

		public override ZString Description => BizO.SelectionDescription(showStatus);

		public ISelectionItem BizO
		{
			get => bizO;
			set
			{
				bizO = value;
				if (BizO is BusinessObject bo)
				{
					RegisterEditableChildObject(bo);
				}
			}
		}
		ISelectionItem bizO;

		public bool BizoHasMessageErrors => BizO is BusinessObject bo && bo.HasMessageErrors;

		public MessageChooser Chooser { get; }

		public AsycudaBill Bill
		{
			get
			{
				if (bill == null && BizO is AsycudaBill asycudaBill)
				{
					bill = asycudaBill;
				}

				return bill;
			}
		}
		AsycudaBill bill;

		readonly bool showStatus;
	}
}
