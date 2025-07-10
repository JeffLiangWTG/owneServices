using System;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;

namespace Enterprise.Customs.GB.CDS
{
	abstract class MovementRequestMessageBuilder : CDSInventoryLinkingRequestMessageBuilder
	{
		protected MovementRequestMessageBuilder(IUkCinvWrapper messageDataProvider, GbInventoryManagementMessageFunction.MasterOrDeclaration level)
			: base(messageDataProvider)
		{
			this.level = level;
		}

		protected sealed override ZString Build()
		{
			var request = new inventoryLinkingMovementRequest
			{
				messageCode = MessageCode,
				goodsLocation = messageDataProvider.LocationOfGoods,
				shedOPID = messageDataProvider.ShedCode,
				movementReference = messageDataProvider.MovementReference,
				transportDetails = new transportDetails
				{
					transportID = messageDataProvider.TransportIdentityAtTheBorderBox21,
					transportMode = messageDataProvider.TransportModeAtTheBorderBox25,
					transportNationality = messageDataProvider.TransportNationalityAtTheBorderBox21
				}
			};

			switch (level)
			{
				case GbInventoryManagementMessageFunction.MasterOrDeclaration.Master:
					request.masterUCR = messageDataProvider.MasterUniqueConsignmentReference;
					request.ucrBlock = new ucrBlock
					{
						ucr = messageDataProvider.MasterUniqueConsignmentReference,
						ucrType = ucrType.M
					};
					break;
				case GbInventoryManagementMessageFunction.MasterOrDeclaration.Declaration:
					request.ucrBlock = UCRHelper.ComposeUcrBlock(messageDataProvider.CDSDeclarationUniqueConsignmentReference,
						messageDataProvider.CDSDeclarationUniqueConsignmentReferencePartSuffix, ucrType.D);
					break;
			}

			var masterOption = messageDataProvider.MasterOpt;
			request.masterOptSpecified = !masterOption.IsEmpty;
			if (request.masterOptSpecified)
			{
				request.masterOpt = GetMasterOpt(messageDataProvider.MasterOpt);
			}

			DoCustomSetting(request);

			return request.Serialize();
		}

		protected virtual void DoCustomSetting(inventoryLinkingMovementRequest request)
		{
		}

		static masterOpt GetMasterOpt(ZString masterOption)
		{
			switch (masterOption)
			{
				case MasterOpt.Codes.A:
					return masterOpt.A;
				case MasterOpt.Codes.F:
					return masterOpt.F;
				case MasterOpt.Codes.R:
					return masterOpt.R;
				case MasterOpt.Codes.X:
					return masterOpt.X;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"'{masterOption}' is not supported."));
			}
		}

		protected abstract messageCodeMovement MessageCode { get; }
		readonly GbInventoryManagementMessageFunction.MasterOrDeclaration level;
	}
}
