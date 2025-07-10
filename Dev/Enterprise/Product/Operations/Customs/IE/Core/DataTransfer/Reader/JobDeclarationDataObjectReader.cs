using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IE.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

[assembly: UsesConstants(typeof(ImportDeclarationApplicationCodeList))]

namespace Enterprise.Customs.IE.DataTransfer.Reader
{
	public class JobDeclarationDataObjectReader : EU.DataTransfer.Universal.JobDeclarationDataObjectReader
	{
		public JobDeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, JobDeclaration declaration) => new CustomsEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);

		protected override void PopulateApplicationCode(UniversalShipment dataObject, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (!declaration.IsInDatabase && dataObject.MessageType is CodeDescriptionPair messageType && messageType.Code.GetValueOrDefault() == JobMessageTypeList.Codes.Import)
			{
				var declarationApplicationCode = (dataObject.MessagingApplicationCode?.Code.GetValueOrDefault().ToString() ?? string.Empty) switch
				{
					ImportDeclarationApplicationCodeList.Codes.V1 => ImportDeclarationApplicationCodeList.Codes.V1,
					ImportDeclarationApplicationCodeList.Codes.V2 => ImportDeclarationApplicationCodeList.Codes.V2,
					_ => declaration.GetInterfaceSubmissionType().ToString() switch
					{
						DeclarationApplicationCodeList.Codes.Builtin => ImportDeclarationApplicationCodeList.Codes.V1,
						DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted => ImportDeclarationApplicationCodeList.Codes.V1,
						_ => ImportDeclarationApplicationCodeList.Codes.Interfaced
					}
				};
				SetValueWithDelay(GetColumnIndexer(declaration), JobDeclarationSchema.JE_ApplicationCode, () => declarationApplicationCode, delaySetters);
			}
			else
			{
				base.PopulateApplicationCode(dataObject, declaration, delaySetters);
			}
		}
	}
}
