using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.PHS11;
using Enterprise.DocumentWrappers;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.MCP.PHS11
{
	[WTG.StaticAnalysis.Annotation.CodeAlive(@"Used by DocumentWrapperFactory.CreateWrapperWithoutException (i.e. Activator.CreateInsteance) in Sea:\dev\Enterprise\Product\Operations\Customs\GB\Core\Business\Declaration\EdiMessageDocumentSupporter.cs")]
	public class DocMcpPhs11Wrapper : DocBaseWrapper
	{
		DocMcpPhs11Wrapper(EDIMessage phsEdiMessageToWrap, BusinessObjectFactory factoryToWrap)
			: base(phsEdiMessageToWrap, factoryToWrap)
		{
			this.ediMessageToWrap = phsEdiMessageToWrap;
			source = new PHS11Message(phsEdiMessageToWrap.EM_MessageText);
		}

		public static DocMcpPhs11Wrapper New(EDIMessage phsEdiMessageToWrap, BusinessObjectFactory factoryToWrap)
		{
			return new DocMcpPhs11Wrapper(phsEdiMessageToWrap, factoryToWrap);
		}

		public ZString DucrAndReference
		{
			get
			{
				var result = ZString.Empty;
				if (ediMessageToWrap != null)
				{
					var linkedEntry = ediMessageToWrap.EM_LinkedObject as CusEntryHeader;
					JobDeclaration dec = null;
					if (linkedEntry != null)
					{
						dec = linkedEntry.Declaration;
						result = linkedEntry.CH_BGMReference;
					}
					else
					{
						dec = ediMessageToWrap.EM_LinkedObject as JobDeclaration;
						result = dec.JE_DeclarationReference;
					}
					if (dec != null && dec.JE_MasterUCR != UCN)
					{
						result = string.Format("{0}\r\nMUCR {1}", result, dec.JE_MasterUCR);
					}
				}
				return result;
			}
		}

		public ZString MessageIdentifier
		{
			get { return source.MessageIdentifier.Replace("=", ""); }
		}

		public ZDateTime CreateDateTime
		{
			get { return source.CreateDateTime; }
		}

		public ZString McpCompanyCode
		{
			get { return source.CompanyCode; }
		}

		public ZString UCN
		{
			get { return source.UCN; }
		}

		public ZString UnitId
		{
			get { return source.UnitId; }
		}

		public ZString LorryId
		{
			get { return source.LorryId; }
		}

		public ZString PortHealthStatusCode
		{
			get { return source.PortHealthStatus; }
		}

		public ZString PortHealthStatusDescription
		{
			get { return new PortHealthStatusCodes().GetDescriptionFromCode(source.PortHealthStatus); }
		}

		public ZString FoodCode
		{
			get { return source.FoodCode; }
		}

		public ZString Berth
		{
			get { return source.Berth; }
		}

		public ZString MarksAndNumbers
		{
			get { return source.MarksAndNumbers; }
		}

		public ZString NominatedAgent
		{
			get { return source.NominatedAgent; }
		}

		public ZString BillOfLading
		{
			get { return source.BillOfLading; }
		}

		public ZString AgentsReference
		{
			get { return source.AgentsReference; }
		}

		public ZInt NumberOfPackages
		{
			get { return ZInt.ParseEmptyAsZero(source.NumberOfPackages); }
		}

		public ZDecimal GrossMass
		{
			get { return ZDecimal.ParseSafe(source.GrossMass, ZDecimal.Zero); }
		}

		public ZString Description
		{
			get { return source.Description; }
		}

		public ZString GeneratingTX
		{
			get { return source.GeneratingTX; }
		}

		public ZString UserName
		{
			get { return source.UserName; }
		}

		readonly PHS11Message source;
		readonly EDIMessage ediMessageToWrap;
	}
}
