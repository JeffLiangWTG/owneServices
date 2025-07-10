using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPremisesIdAndProcessingTypeValidation : ZValidation
	{
		public AQISPremisesIdAndProcessingTypeValidation(AQISPremisesIdAndProcessingType parent)
			: base(parent)
		{
			this.aQISPremisesIdAndProcessingType = parent;
		}

		public override void ValidateAll()
		{
			ValidatePremisesId();
			ValidateProcessingType();
		}

		public override Type AutoValidationType
		{
			get { return typeof(AQISPremisesIdAndProcessingTypeValidation); }
		}

		#region PremisesId

		public void ValidatePremisesId()
		{
			ValidateCalculatedProperty(aQISPremisesIdAndProcessingType.PremisesIdInfo);
		}

		protected void CheckPremisesId()
		{
			if (!aQISPremisesIdAndProcessingType.IsValidationSuspended)
			{
				ListValidation.MessageErrorIfInvalidCode(aQISPremisesIdAndProcessingType.PremisesIdInfo);

				if (aQISPremisesIdAndProcessingType.PremisesId.Contains('/') || aQISPremisesIdAndProcessingType.PremisesId.Contains(','))
				{
					aQISPremisesIdAndProcessingType.PremisesIdInfo.AddError("You cannot use the characters '/' or ','");
				}

				ValidateOnly10Records();
				ValidateEmptyRecords();
			}
		}

		#endregion

		#region ProcessingType

		public void ValidateProcessingType()
		{
			ValidateCalculatedProperty(aQISPremisesIdAndProcessingType.ProcessingTypeInfo);
		}

		protected void CheckProcessingType()
		{
			if (!aQISPremisesIdAndProcessingType.IsValidationSuspended)
			{
				ListValidation.MessageErrorIfInvalidCode(aQISPremisesIdAndProcessingType.ProcessingTypeInfo, aQISPremisesIdAndProcessingType.Lookups.AQISProcessingTypeList);

				if (aQISPremisesIdAndProcessingType.ProcessingType.Contains('/') || aQISPremisesIdAndProcessingType.ProcessingType.Contains(','))
				{
					aQISPremisesIdAndProcessingType.ProcessingTypeInfo.AddError("You cannot use the characters '/' or ','");
				}

				ValidateOnly10Records();

				if (aQISPremisesIdAndProcessingType.ProcessingType.EndsWith("D", StringComparison.Ordinal))
				{
					aQISPremisesIdAndProcessingType.ProcessingTypeInfo.AddWarning("Since the introduction of the Integrated Cargo System (ICS), a number of brokers have selected incorrect Quarantine Processing Types for Automatic Entry Processing (AEP)." +
						"The list contains Quarantine Processing Types for non-commodity concerns only and " +
						"non-commodity and commodity concerns combined - these Processing Types end in ‘D’ (meaning commodity ‘DOCS OK’) or ‘ND’ (meaning commodity ‘DOCS NOT OK’)." +
						"Quarantine Processing Types that end in ‘D’ or ‘ND’ should only be used by brokers lodging Import Declarations under an AEP for Commodities Scheme." +
						"The Quarantine Containerised Cargo Clearance for FCL/X, Automatic Entry Processing for FCL and LCL Packing Schemes apply to non-commodity concerns only.  Brokers operating under these Schemes only must not select Quarantine Processing Types ending in ‘D’ or ‘ND’.");
				}
				ValidateEmptyRecords();
			}
		}

		#endregion

		void ValidateOnly10Records()
		{
			if (aQISPremisesIdAndProcessingType.ParentCollections.Count > 0)
			{
				aQISPremisesIdAndProcessingType.ClearRowNotifications();

				AQISPremisesIdAndProcessingTypeCollection premisesIdAndProcessingTypes = ((AQISPremisesIdAndProcessingTypeCollection)aQISPremisesIdAndProcessingType.ParentCollections.First());

				if (premisesIdAndProcessingTypes != null && premisesIdAndProcessingTypes.Count > 10 && (!premisesIdAndProcessingTypes[10].PremisesId.IsEmpty || !premisesIdAndProcessingTypes[10].ProcessingType.IsEmpty))
				{
					aQISPremisesIdAndProcessingType.AddRowError("You can only enter 10 AQIS Premises Id's and Processing Types");
				}
			}
		}

		void ValidateEmptyRecords()
		{
			if (aQISPremisesIdAndProcessingType.PremisesId.IsEmpty && aQISPremisesIdAndProcessingType.ProcessingType.IsEmpty)
			{
				aQISPremisesIdAndProcessingType.AddRowMessageError("This field contains a blank value so will not be sent in the message. If this is not correct, delete this AEP record and save before resubmitting.");
			}
		}

		readonly AQISPremisesIdAndProcessingType aQISPremisesIdAndProcessingType;
	}
}
