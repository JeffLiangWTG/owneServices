using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class DeclarationDataObjectReader : EU.DataTransfer.Universal.JobDeclarationDataObjectReader
	{
		public DeclarationDataObjectReader(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override void PopulateLocationAtClearanceForReader(UniversalShipment dataObject, EU.Business.Declaration.JobDeclaration baseDeclaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declaration = baseDeclaration as JobDeclaration;
			if (declaration != null)
			{
				var declarationRow = GetColumnIndexer(declaration);
				var locationOtherInformation = declaration.ApplicationExtender.DataTransferHelper.GetLocationOtherInformationFromLocationAtClearanceForReadingUXML(dataObject.LocationAtClearance);
				SetValue(declarationRow, JobDeclarationSchema.JE_LocationOtherInformation, locationOtherInformation, delaySetters);

				var locationQualifier = declaration.ApplicationExtender.DataTransferHelper.GetLocationQualifierFromLocationAtClearanceForReadingUXML(dataObject.LocationAtClearance);
				SetValue(declarationRow, JobDeclarationSchema.JE_LocationQualifier, locationQualifier, delaySetters);

				using (declaration.SetterSuspender.ResumeSetting(nameof(declaration.JE_LocationOfGoods)))
				{
					var locationOfGoods = declaration.ApplicationExtender.DataTransferHelper.GetLocationOfGoodsFromLocationAtClearanceForReadingUXML(dataObject.LocationAtClearance);
					SetValue(declarationRow, JobDeclarationSchema.JE_LocationOfGoods, locationOfGoods);
				}
			}
		}

		protected override void PopulateApplicationCode(UniversalShipment dataObject, EU.Business.Declaration.JobDeclaration baseDeclaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (!baseDeclaration.IsInDatabase)
			{
				var declaration = baseDeclaration as JobDeclaration;
				var applicationCode = dataObject.MessagingApplicationCode;
				if (declaration != null && applicationCode != null)
				{
					var declarationRow = GetColumnIndexer(declaration);
					using (declaration.SetterSuspender.ResumeSetting(nameof(declaration.JE_ApplicationCode)))
					{
						SetValue(declarationRow, JobDeclarationSchema.JE_ApplicationCode, applicationCode);
					}
				}
			}
		}

		protected override void PopulateCustomsProfile(UniversalShipment dataObject, EU.Business.Declaration.JobDeclaration baseDeclaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var declaration = baseDeclaration as JobDeclaration;
			if (declaration != null)
			{
				using (declaration.SetterSuspender.ResumeSetting(nameof(declaration.JE_CustomsProfile)))
				{
					base.PopulateCustomsProfile(dataObject, declaration, delaySetters);
				}
			}
		}

		protected override bool CanPopulateDeclarationWhenMessageSent => true;

		protected override DateTypeSchemaColumnMap[] GetDateFieldsToReadWhichDontNeedEstimatedFirst()
		{
			return base.GetDateFieldsToReadWhichDontNeedEstimatedFirst().Concat(Enumerable.Repeat(new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_EntryAuthorisationDate, DateType.EntryAuthorisation), 1)).ToArray();
		}

		protected override void FillCountrySpecificDetails(EU.Business.Declaration.JobDeclaration declaration, UniversalShipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillCountrySpecificDetails(declaration, dataObject, delaySetters);

			var masterUCR = dataObject.EntryNumberCollection?
				.Where(entryNumber => entryNumber.Type.Code.Value == CusEntryNumberTypes.EU.MasterUCR)
				.Select(entryNumber => entryNumber.Number)
				.FirstOrDefault();
			if (masterUCR.HasValue && !masterUCR.Value.IsEmpty)
			{
				var target = declaration as JobDeclaration;
				if (target != null)
				{
					target.JE_MasterUCR = masterUCR.Value;
				}
			}
		}

		protected override IEnumerable<ZString> GetDeclarationPropertiesToSuspendSetting()
		{
			foreach (var property in base.GetDeclarationPropertiesToSuspendSetting())
			{
				yield return property;
			}
			if (dataObject.MessagingApplicationCode != null)
			{
				yield return JobDeclaration.Schema.JE_ApplicationCode;
			}
			if (dataObject.CustomsProfileIdentifier != null)
			{
				yield return JobDeclaration.Schema.JE_CustomsProfile;
			}
		}

		protected override bool CanPopulateDeclaration(EU.Business.Declaration.JobDeclaration declaration)
		{
			if (!base.CanPopulateDeclaration(declaration))
			{
				return false;
			}
			var applicationCode = dataObject.MessagingApplicationCode?.Code.GetValueOrDefault() ?? ZString.Empty;

			var profile = dataObject.CustomsProfileIdentifier?.Value.GetValueOrDefault() ?? ZString.Empty;
			var messageType = CalculateMessageCode(dataObject, declaration);
			var branch = dataObject.GetBranchPK(factory.BOFactory);
			if (!branch.IsValid)
			{
				branch = declaration.RegistryBranchPK;
			}
			var collection = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), branch.ToGuid(), Guid.Empty);
			var defaultApplicationCode = collection.FindByBadgeCode(profile, messageType)?.ApplicationCode ?? ZString.Empty;
			if ((declaration.IsInDatabase && declaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF) ||
				applicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF ||
				(applicationCode == ZString.Empty && defaultApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF))
			{ 
				throw new MessageProcessingBusinessFailureException("Save aborted because this is a CHIEF declaration", declaration.JE_DeclarationReference, false);
			}
			return true;
		}
	}
}
