using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUUnderbondData),
	Enterprise.Core.Constants.DocManagerCodes.CusUnderbond,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	class AUUnderbondData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CusUnderbond); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return "UNB"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("06cba60a-9616-4633-ab0f-365d7c2b5a07", "Underbond"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new AUUnderbondDataEDocsViaUniversalXmlSupport();
	}
}
