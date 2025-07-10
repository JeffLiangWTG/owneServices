using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class UnloadingRemarkWrapper : IUnloadingRemarkInterface
	{
		public UnloadingRemarkWrapper(UnloadingRemarkAddInfo unloadingRemark)
		{
			this.unloadingRemark = Argument.NotNull(unloadingRemark, nameof(unloadingRemark));
		}
		public ZString StateOfSealsOk => unloadingRemark.G9_StateOfSealsOk;

		public ZString Conform => unloadingRemark.G9_Conform;

		public ZString UnloadingCompletion => unloadingRemark.G9_UnloadingCompletion;

		public ZString UnloadingDate => WrapperHelper.GetLongDate(unloadingRemark.G9_UnloadingDate);

		public ZInt NoOfSeals => unloadingRemark.G9_StateOfSealsOk == "N" ? unloadingRemark.G9_NoOfSeals : ZInt.Zero;

		public ZInt RawNoOfSeals => unloadingRemark.G9_NoOfSeals;

		readonly UnloadingRemarkAddInfo unloadingRemark;
	}
}
