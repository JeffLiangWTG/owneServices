using System.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class RFPMessageStreamFormatter : EDIMessageStreamFormatter
	{
		const string startBlockString = "PNA+AV+";
		const string endBlockString = "\r\n";
		const string replaceString = "******";

		public override void FormatStream(ref Stream stream)
		{
			base.FormatStream(ref stream);
			stream = stream.ReplaceTextBlock(startBlockString, endBlockString, replaceString);
		}
	}
}
