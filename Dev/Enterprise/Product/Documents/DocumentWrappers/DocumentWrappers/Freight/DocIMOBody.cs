using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	[AllowPublicConstructor, AllowNoStaticNew]
	public class DocIMOBody : DocumentWrapper, IObsoleteValidation
	{
		public DocIMOBody(ZString marksLine, ZString goodsDescLine, ZString grossMassLine, ZString netMassLine, ZString cubeLine)
		{
			fMarksLine = marksLine;
			fGoodsDescLine = goodsDescLine;
			fGrossMassLine = grossMassLine;
			fNetMassLine = netMassLine;
			fCubeLine = cubeLine;
		}

		public override string ToString() { return ""; }
		protected ZString fMarksLine;
		public ZString MarksAndNumbers
		{
			get { return fMarksLine; }
		}
		protected ZString fGoodsDescLine;
		public ZString GoodsDescription
		{
			get { return fGoodsDescLine; }
		}
		protected ZString fGrossMassLine;
		public ZString GrossMass
		{
			get { return fGrossMassLine; }
		}
		protected ZString fNetMassLine;
		public ZString NetMassLine
		{
			get { return fNetMassLine; }
		}
		protected ZString fCubeLine;
		public ZString Volume
		{
			get { return fCubeLine; }
		}
	}
}
