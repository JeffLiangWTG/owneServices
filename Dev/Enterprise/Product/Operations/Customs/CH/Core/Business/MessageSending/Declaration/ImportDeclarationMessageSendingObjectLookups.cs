using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class ImportDeclarationMessageSendingObjectLookups : DeclarationMessageSendingObjectLookups
{
	public ImportDeclarationMessageSendingObjectLookups(BusinessObject parent) : base(parent)
	{
	}

	public new DeclarationMessageSendingObject Parent => (DeclarationMessageSendingObject)base.Parent;

	public override CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var status = Parent.Header.CH_Status;
			var phaseStatus = Parent.Header.CH_PhaseStatus;

			return Factory.GetCachedValue($"CH.DeclarationMessageSendingObjectLookups.MessageTypeList.{status}.{phaseStatus}", () =>
			{
				var fullList = Factory.GetCachedValue<PassarMessageTypeList>();
				var list = new CodeDescriptionPairList();
				foreach (var code in GetApplicableMessageType(status, phaseStatus))
				{
					list.AddPair(code, fullList.GetDescriptionFromCode(code));
				}
				return list;
			});
		}
	}

	IEnumerable<string> GetApplicableMessageType(ZString status, ZString phaseStatus)
	{
		switch (status)
		{
			case "":
				return new[] { PassarMessageTypeList.Codes.NI015 };
			case CHLogicalStatusList.Codes.Sent:
				return new[] { PassarMessageTypeList.Codes.NI016 };
			case CHLogicalStatusList.Codes.Accepted:
				switch (phaseStatus)
				{
					case PassarDeclarationPhaseList.Codes.Cancellation:
						return new[] { PassarMessageTypeList.Codes.NI016 };
					case PassarDeclarationPhaseList.Codes.Declaration:
						return new[] { PassarMessageTypeList.Codes.NI013, PassarMessageTypeList.Codes.NI014, PassarMessageTypeList.Codes.NI016 };
					default:
						return Array.Empty<string>();
				}
			case CHLogicalStatusList.Codes.Acknowledged:
				switch (phaseStatus)
				{
					case PassarDeclarationPhaseList.Codes.Amendment:
					case PassarDeclarationPhaseList.Codes.Cancellation:
						return new[] { PassarMessageTypeList.Codes.NI016 };
					default:
						return Array.Empty<string>();
				}
			case CHLogicalStatusList.Codes.Invalid:
			case CHLogicalStatusList.Codes.Failed:
				switch (phaseStatus)
				{
					case PassarDeclarationPhaseList.Codes.Amendment:
						return new[] { PassarMessageTypeList.Codes.NI013, PassarMessageTypeList.Codes.NI014 };
					case PassarDeclarationPhaseList.Codes.Cancellation:
						return new[] { PassarMessageTypeList.Codes.NI014, PassarMessageTypeList.Codes.NI013 };
					case PassarDeclarationPhaseList.Codes.Declaration:
						return new[] { PassarMessageTypeList.Codes.NI015 };
					default:
						return Array.Empty<string>();
				}
			default:
				return Array.Empty<string>();
		}
	}

	public override CodeDescriptionPairList CorrectionReasonList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CorrectionReason, ZDate.Today);
}
