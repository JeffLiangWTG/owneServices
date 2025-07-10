using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class MyAccountWebContractParser : DocumentParser<MyAccountWebContract>
	{
		public MyAccountWebContractParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocMyAccountWebContract); }
		}
	}
}
