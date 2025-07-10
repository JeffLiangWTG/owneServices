namespace Enterprise.DataTransfer.SystemMerge.GUI
{
	using Enterprise.DataTransfer.Business;
	using Enterprise.DataTransfer.Integration;

	public abstract class SysMergeXmlDataTransferDirector : XmlDataTransferDirector
	{
		protected SysMergeXmlDataTransferDirector(IValueObjectDataAdapter adapter)
			: base(adapter, true)
		{
		}

		public override bool IsPermitted
		{
			get { return true; }
		}
	}
}
