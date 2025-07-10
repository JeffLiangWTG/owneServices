using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsPopulatePickStrategy : WhsPopulateStrategy
	{
		#region Constructor

		public WhsPopulatePickStrategy(WhsPick pick)
			: base(pick)
		{
		}

		#endregion

		#region Properties

		public override LabelValuePairWrapper PickMethod
		{
			get
			{
				ZString method = ZString.Empty;
				switch (PickBO.WP_PickOption)
				{
					case WhsPickOption.Codes.Auto:
						method = WhsPickOption.Descriptions.Auto;
						break;

					case WhsPickOption.Codes.Manual:
						method = WhsPickOption.Descriptions.Manual;
						break;

					case WhsPickOption.Codes.ManualWithAutoAllocate:
						method = WhsPickOption.Descriptions.ManualWithAutoAllocate;
						break;
				}
				return new LabelValuePairWrapper(Res.GetString("acbcaacc-77f3-4326-8b8a-a9cca2550e7c", "Pick Method"), method.ToUpper(), Factory);
			}
		}

		#endregion

		#region Implementation

		WhsPick PickBO
		{
			get { return pickBO ?? (pickBO = (WhsPick)WrappedBO); }
		}
		WhsPick pickBO;

		#endregion
	}
}
