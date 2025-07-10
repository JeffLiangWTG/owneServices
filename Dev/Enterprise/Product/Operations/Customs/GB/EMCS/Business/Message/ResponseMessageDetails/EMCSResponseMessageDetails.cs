using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class EMCSResponseMessageDetails
	{
		EMCSResponseMessageDetails() { }

		public static EMCSResponseMessageDetails Instance => instance ??= new EMCSResponseMessageDetails();
		[ThreadStatic]
		static EMCSResponseMessageDetails instance;

		public ImmutableDictionary<ZString, EMCSResponseDetail[]> ResponseMessages => responseMessages ?? (responseMessages = ImmutableDictionary.CreateRange(new Dictionary<ZString, EMCSResponseDetail[]>
		{
			{ EMCSGBIncomingMessageTypeList.Codes.IE704, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie704uk.Ie704Type), typeof(IE704MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE801, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801.Ie801Type), typeof(IE801MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE802, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie802.Ie802Type), typeof(IE802MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE803, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie803.Ie803Type), typeof(IE803MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE807, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie807.Ie807Type), typeof(IE807MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE810, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie810.Ie810Type), typeof(IE810MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE813, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813.Ie813Type), typeof(IE813MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE818, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818.Ie818Type), typeof(IE818MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE819, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie819.Ie819Type), typeof(IE819MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE829, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie829.Ie829Type), typeof(IE829MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE837, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie837.Ie837Type), typeof(IE837MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE839, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie839.Ie839Type), typeof(IE839MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE840, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie840.Ie840Type), typeof(IE840MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE871, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie871.Ie871Type), typeof(IE871MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE881, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881.Ie881Type), typeof(IE881MessageProcessor)) } },
			{ EMCSGBIncomingMessageTypeList.Codes.IE905, new EMCSResponseDetail[] { new (typeof(CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie905.Ie905Type), typeof(IE905MessageProcessor)) } },
		}));

		ImmutableDictionary<ZString, EMCSResponseDetail[]> responseMessages;

		public EMCSResponseDetail? GetResponseMessage(string messageType)
		{
			return ResponseMessages.TryGetValue(messageType, out var responseType) ? responseType[0] : null;
		}
	}
}
