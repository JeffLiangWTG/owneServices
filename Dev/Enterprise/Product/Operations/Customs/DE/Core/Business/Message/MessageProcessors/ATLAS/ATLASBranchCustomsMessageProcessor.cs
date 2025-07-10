using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.ESumA;
using Enterprise.Customs.DE.Business.Import;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business
{
	public class ATLASBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		public ATLASBranchCustomsMessageProcessor()
			: base(new ZString[] { EDIMessage.ApplicationCodes.DECustomsAtlasSystem }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			var nctsMessageProcessors = ObjectFactory.Get<Integration.Customs.DE.INctsMessageProcessor>().NctsMessageProcessors;
			var allAtlasMessageProcessors = AtlasMessageProcessors.AddRange(nctsMessageProcessors);
			BranchCustomsApplicationTypeMessageProcessor result = null;

			var applicationReference = message.EM_ApplicationReference;
			if (!applicationReference.IsEmpty)
			{
				var key = applicationReference.SubstringSafe(0, 5).ToString();

				if (allAtlasMessageProcessors.TryGetValue(key, out var processorType))
				{
					var obj = Activator.CreateInstance(processorType, Logger);
					if (obj is BranchCustomsApplicationTypeMessageProcessor)
					{
						result = (BranchCustomsApplicationTypeMessageProcessor)obj;
					}
					else if (obj is IDEBranchCustomsApplicationTypeMessageProcessorProvider provider)
					{
						result = provider.GetProcessor(message);
					}
				}
			}
			return result;
		}

		ImmutableDictionary<string, Type> atlasMessageProcessors;
		ImmutableDictionary<string, Type> AtlasMessageProcessors
		{
			get
			{
				if (atlasMessageProcessors == null)
				{
					atlasMessageProcessors = new Dictionary<string, Type>
					{
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCRECF).Substring(0, 5), typeof(CUSRECMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DEERRF).Substring(0, 5), typeof(ERRNCKMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCTRAG).Substring(0, 5), typeof(CUSTRAMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCNOAD).Substring(0, 5), typeof(CUSNOAMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCTAXM).Substring(0, 5), typeof(CUSTAXMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GNTAXK).Substring(0, 5), typeof(NFFTAXMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCRELG).Substring(0, 5), typeof(ImportCURRELMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCTSTJ).Substring(0, 5), typeof(TemporaryStorageCUSTSTMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCSTPC).Substring(0, 5), typeof(TemporaryStorageCUSSTPMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCCANE).Substring(0, 5), typeof(TemporaryStorageCUSCANMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCFING).Substring(0, 5), typeof(TemporaryStorageCUSFINMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCFSTF).Substring(0, 5), typeof(TemporaryStorageCUSFSTMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCSTAB).Substring(0, 5), typeof(TemporaryStorageCUSSTAMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DEIACA).Substring(0, 5), typeof(TemporaryStorageENSCTLMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DEIESB).Substring(0, 5), typeof(ESumAENSSTAMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DEISID).Substring(0, 5), typeof(ESumAAIVNOTMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DEISAC).Substring(0, 5), typeof(ESumAENSACKMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.FCREVHCancellationText).Substring(0, 5), typeof(ImportCUSREVMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.FFTAXE).Substring(0, 5), typeof(ImportFINTAXMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LECWIF).Substring(0, 5), typeof(ECWINFMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NSREVC).Substring(0, 5), typeof(ImportSRAREVMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NSTAXK).Substring(0, 5), typeof(ImportSRATAXMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LSCWIF).Substring(0, 5), typeof(ImportSCWINFMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCWSIE).Substring(0, 5), typeof(ImportCWSINFMessageProcessor) },
					}.ToImmutableDictionary();
				}
				return atlasMessageProcessors;
			}
		}
	}
}
