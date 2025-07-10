using CargoWise.Types;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.MCP.PHS11
{
	public class PHS11Message : IUcnProvider
	{
		public PHS11Message(ZString commaSeparatedText)
		{
			var elements = commaSeparatedText.Split('~');
			MessageIdentifier = elements[0];
			ZDateTime tempDate;
			ZDateTime.TryParseExact(elements[1], out tempDate, "yyMMddHHmm");
			CreateDateTime = tempDate;
			CompanyCode = elements[2];
			UCN = elements[3];  // NB, twelve characters only !
			UnitId = elements[4];
			LorryId = elements[5];
			PortHealthStatus = elements[6];
			FoodCode = elements[7];
			Berth = elements[8];
			MarksAndNumbers = elements[9];
			NominatedAgent = elements[10];
			BillOfLading = elements[11];
			AgentsReference = elements[12];
			NumberOfPackages = elements[13];
			GrossMass = elements[14];
			Description = elements[15];
			GeneratingTX = elements[16];
			UserName = elements[17].Replace("}", "");
			if (elements.Length == 19)
			{
				BillOfLading = elements[18].Replace("}", ""); // longer field for post-ICS BoLs
			}
		}

		ZString IUcnProvider.UcnNumberProperlyTruncated
		{
			get { return UCN.EndsWith("000") ? UCN.Left(9) : UCN; }
		}

		ZString IUcnProvider.UcnNumberVerbatim
		{
			get { return UCN; }
		}

		public ZString MessageIdentifier { get; private set; }
		public ZDateTime CreateDateTime { get; private set; }
		public ZString CompanyCode { get; private set; }
		public ZString UCN { get; private set; }
		public ZString UnitId { get; private set; }
		public ZString LorryId { get; private set; }
		public ZString PortHealthStatus { get; private set; }
		public ZString FoodCode { get; private set; }
		public ZString Berth { get; private set; }
		public ZString MarksAndNumbers { get; private set; }
		public ZString NominatedAgent { get; private set; }
		public ZString BillOfLading { get; private set; }
		public ZString AgentsReference { get; private set; }
		public ZString NumberOfPackages { get; private set; }
		public ZString GrossMass { get; private set; }
		public ZString Description { get; private set; }
		public ZString GeneratingTX { get; private set; }
		public ZString UserName { get; private set; }
	}
}
