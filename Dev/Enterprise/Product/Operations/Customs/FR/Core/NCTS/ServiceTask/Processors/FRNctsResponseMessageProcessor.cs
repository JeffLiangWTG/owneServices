using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class FRNctsResponseMessageProcessor : BaseMessageProcessor
	{
		const string messageStatusQueued = "QUE";
		const string messageApplicationCode = "NCT";

		public FRNctsResponseMessageProcessor(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();

			result.Add(new DTCC004AProcessor(serviceLogger, Logger));
			result.Add(new DTCC005AProcessor(serviceLogger, Logger));
			result.Add(new DTCC008AProcessor(serviceLogger, Logger));
			result.Add(new DTCC009AProcessor(serviceLogger, Logger));
			result.Add(new DTCC013BProcessor(serviceLogger, Logger));
			result.Add(new DTCC014AProcessor(serviceLogger, Logger));
			result.Add(new DTCC015BProcessor(serviceLogger, Logger));
			result.Add(new DTCC016AProcessor(serviceLogger, Logger));
			result.Add(new DTCC019AProcessor(serviceLogger, Logger));
			result.Add(new DTCC021AProcessor(serviceLogger, Logger));
			result.Add(new DTCC025AProcessor(serviceLogger, Logger));
			result.Add(new DTCC028AProcessor(serviceLogger, Logger));
			result.Add(new DTCC029BProcessor(serviceLogger, Logger));
			result.Add(new DTCC035AProcessor(serviceLogger, Logger));
			result.Add(new DTCC043AProcessor(serviceLogger, Logger));
			result.Add(new DTCC044AProcessor(serviceLogger, Logger));
			result.Add(new DTCC045AProcessor(serviceLogger, Logger));
			result.Add(new DTCC055AProcessor(serviceLogger, Logger));
			result.Add(new DTCC058AProcessor(serviceLogger, Logger));
			result.Add(new DTCC140AProcessor(serviceLogger, Logger));
			result.Add(new DTCC141AProcessor(serviceLogger, Logger));
			result.Add(new DTCCF02AProcessor(serviceLogger, Logger));
			result.Add(new DTCCF03AProcessor(serviceLogger, Logger));
			result.Add(new DTCCF15AProcessor(serviceLogger, Logger));
			result.Add(new DTCCF96AProcessor(serviceLogger, Logger));
			result.Add(new DTCCF97AProcessor(serviceLogger, Logger));
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			LoggingInformation logging = new LoggingInformation();

			XmlDocument xml = new XmlDocument();
			xml.LoadXml(message.EM_MessageText);

			var messageType = ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "MesTypMES20");
			var jobNumber = ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "MesIdeMES19");

			var nctsHeader = FindParentHeaderFromId(jobNumber, message.Factory);

			if (nctsHeader == null)
			{
				serviceLogger.Log(LogType.Error, ZString.Format("Cannot find NCTS movement with reference number {0}, message {1} has been discarded.", jobNumber, message.EM_MessageNum));
			}
			else
			{
				switch (messageType)
				{
					case MessageTypeList.Codes.CC016A:  // Declaration rejected

						var procCC016A = new DTCC016AProcessor(serviceLogger, logging);
						procCC016A.NctsHeader = nctsHeader;

						return procCC016A;

					case MessageTypeList.Codes.CC028A:  // MRN allocated

						var procCC028A = new DTCC028AProcessor(serviceLogger, logging);
						procCC028A.NctsHeader = nctsHeader;

						return procCC028A;

					case MessageTypeList.Codes.CC029B:  // Release for departure

						var procCC029B = new DTCC029BProcessor(serviceLogger, logging);
						procCC029B.NctsHeader = nctsHeader;

						return procCC029B;

					case MessageTypeList.Codes.CCF97A:  // Technical rejections

						var proCCF97A = new DTCCF97AProcessor(serviceLogger, logging);
						proCCF97A.NctsHeader = nctsHeader;

						return proCCF97A;

					case MessageTypeList.Codes.CCF96A:  // Functional rejection

						var proCCF96A = new DTCCF96AProcessor(serviceLogger, logging);
						proCCF96A.NctsHeader = nctsHeader;

						return proCCF96A;

					case MessageTypeList.Codes.CC008A:  // Arrival rejected

						var proCC008A = new DTCC008AProcessor(serviceLogger, logging);
						proCC008A.NctsHeader = nctsHeader;

						return proCC008A;

					case MessageTypeList.Codes.CCF03A:  // Arrival acceptance

						var proCCF03A = new DTCCF03AProcessor(serviceLogger, logging);
						proCCF03A.NctsHeader = nctsHeader;

						return proCCF03A;

					case MessageTypeList.Codes.CC025A:  // Goods released

						var proCC025A = new DTCC025AProcessor(serviceLogger, logging);
						proCC025A.NctsHeader = nctsHeader;

						return proCC025A;

					case MessageTypeList.Codes.CC043A:  // Unloading permission

						var proCC043A = new DTCC043AProcessor(serviceLogger, logging);
						proCC043A.NctsHeader = nctsHeader;

						return proCC043A;

					case MessageTypeList.Codes.CC005A:  // Amendment rejected

						var proCC005A = new DTCC005AProcessor(serviceLogger, logging);
						proCC005A.NctsHeader = nctsHeader;

						return proCC005A;

					case MessageTypeList.Codes.CC004A: // Amendment accepted

						var proCC004A = new DTCC004AProcessor(serviceLogger, logging);
						proCC004A.NctsHeader = nctsHeader;

						return proCC004A;

					case MessageTypeList.Codes.CC009A:  // Cancellation decision

						var proCC009A = new DTCC009AProcessor(serviceLogger, logging);
						proCC009A.NctsHeader = nctsHeader;

						return proCC009A;

					case MessageTypeList.Codes.CC055A:  // Guarantees not valid

						var proCC055A = new DTCC055AProcessor(serviceLogger, logging);
						proCC055A.NctsHeader = nctsHeader;

						return proCC055A;

					case MessageTypeList.Codes.CC035A:  // Guarantees query

						var proCC035A = new DTCC035AProcessor(serviceLogger, logging);
						proCC035A.NctsHeader = nctsHeader;

						return proCC035A;

					case MessageTypeList.Codes.CC140A:  // Non-arrival query

						var proCC140A = new DTCC140AProcessor(serviceLogger, logging);
						proCC140A.NctsHeader = nctsHeader;

						return proCC140A;

					case MessageTypeList.Codes.CC019A:  // Discrepancies

						var proCC019A = new DTCC019AProcessor(serviceLogger, logging);
						proCC019A.NctsHeader = nctsHeader;

						return proCC019A;

					case MessageTypeList.Codes.CC045A:  // Write off notification 

						var proCC045A = new DTCC045AProcessor(serviceLogger, logging);
						proCC045A.NctsHeader = nctsHeader;

						return proCC045A;

					case MessageTypeList.Codes.CCF02A:  // Status change

						var proCCF02A = new DTCCF02AProcessor(serviceLogger, logging);
						proCCF02A.NctsHeader = nctsHeader;

						return proCCF02A;

					case MessageTypeList.Codes.CC058A:  // Unloading remarks rejected

						var proCC058A = new DTCC058AProcessor(serviceLogger, logging);
						proCC058A.NctsHeader = nctsHeader;

						return proCC058A;

					case MessageTypeList.Codes.CC021A:  // Divert to alternate destination rejection

						var proCC021A = new DTCC021AProcessor(serviceLogger, logging);
						proCC021A.NctsHeader = nctsHeader;

						return proCC021A;

					case MessageTypeList.Codes.CCF15A:  // Declaration de transit

						var proCCF15A = new DTCCF15AProcessor(serviceLogger, logging);
						proCCF15A.NctsHeader = nctsHeader;

						return proCCF15A;

					default:
						return null;
				}
			}

			return null;
		}

		protected override ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				//Don't get messages linked to France branches but get queued FR messages
				return CountrySpecificNctsMessages(Core.Constants.CountryCodes.France);
			}
		}

		protected ZQuery CountrySpecificNctsMessages(ZString nctsDomainCountryCode)
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_MessageType, nctsDomainCountryCode);
			result.AddToFilter(EDIMessageSchema.EM_Status, messageStatusQueued);
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, messageApplicationCode);
			return result;
		}

		NctsHeader FindParentHeaderFromId(ZString jobNumber, BusinessObjectFactory factory)
		{
			NctsHeader parentBusinessObject = null;

			if (jobNumber.IsEmpty)
			{
				return parentBusinessObject;
			}

			parentBusinessObject = factory.LoadTop1<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_JobReference, jobNumber));

			return parentBusinessObject;
		}

		readonly ILogger serviceLogger;
	}
}
