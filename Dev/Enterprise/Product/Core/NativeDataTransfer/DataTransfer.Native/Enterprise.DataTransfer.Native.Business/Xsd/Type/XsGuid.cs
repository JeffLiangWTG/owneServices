namespace Enterprise.DataTransfer.Native.Business.Xsd.Type
{
	public class XsGuid : XsdDataType
	{
		public override string Name
		{
			get { return "xs:string"; } // xs:guid not supported by W3C 
		}
	}
}