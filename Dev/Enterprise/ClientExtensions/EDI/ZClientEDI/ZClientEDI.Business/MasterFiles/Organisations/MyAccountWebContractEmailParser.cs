using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class MyAccountWebContractEmailParser : DocumentParser<MyAccountWebContract>
	{
		public MyAccountWebContractEmailParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocMyAccountWebContractEmail); }
		}
	}
}
