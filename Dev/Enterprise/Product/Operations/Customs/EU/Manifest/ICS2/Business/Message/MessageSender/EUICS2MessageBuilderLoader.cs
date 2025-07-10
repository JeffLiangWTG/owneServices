using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class EUICS2MessageBuilderLoader
	{
		EUICS2MessageBuilderLoader() { }
		public static EUICS2MessageBuilderLoader Instance => instance ?? (instance = new EUICS2MessageBuilderLoader());
		[ThreadStatic]
		static EUICS2MessageBuilderLoader instance;

		public IMessageBuilder GetMessageBuilder(ZString messageCode, AsycudaManifestHeader manifestHeader, IAmendedItem[] amendedItems)
		{
			IMessageBuilder result = null;

			if (messageBuilders.TryGetValue(messageCode, out var messageBuilderDetails))
			{
				var dataProvider = (IICS2MessageHeader)Activator.CreateInstance(messageBuilderDetails.MessageHeaderProviderType, manifestHeader);

				if (dataProvider != null)
				{
					if (dataProvider is IAmendedItemsProvider amendedItemsProvider)
					{
						amendedItemsProvider.AmendedItems = amendedItems;
					}

					result = (IMessageBuilder)Activator.CreateInstance(messageBuilderDetails.MessageBuilderType, dataProvider);
				}
			}

			if (result == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid EU ICS2 Message Builder for code: {messageCode}"));
			}
			return result;
		}

		readonly ImmutableDictionary<ZString, MessageBuilderDetails> messageBuilders = new Dictionary<ZString, MessageBuilderDetails>
		{
			{ MessageTypes.Codes.F10, new MessageBuilderDetails(typeof(F10MessageBuilder), typeof(F10HeaderProvider)) },
			{ MessageTypes.Codes.F13, new MessageBuilderDetails(typeof(F13MessageBuilder), typeof(F13HeaderProvider)) },
			{ MessageTypes.Codes.F14, new MessageBuilderDetails(typeof(F14MessageBuilder), typeof(F14HeaderProvider)) },
			{ MessageTypes.Codes.F15, new MessageBuilderDetails(typeof(F15MessageBuilder), typeof(F15HeaderProvider)) },
			{ MessageTypes.Codes.F16, new MessageBuilderDetails(typeof(F16MessageBuilder), typeof(F16HeaderProvider)) },
			{ MessageTypes.Codes.F17, new MessageBuilderDetails(typeof(F17MessageBuilder), typeof(F17HeaderProvider)) },
			{ MessageTypes.Codes.F22, new MessageBuilderDetails(typeof(F22MessageBuilder), typeof(F22HeaderProvider)) },
			{ MessageTypes.Codes.F23, new MessageBuilderDetails(typeof(F23MessageBuilder), typeof(F23HeaderProvider)) },
			{ MessageTypes.Codes.F24, new MessageBuilderDetails(typeof(F24MessageBuilder), typeof(F24HeaderProvider)) },
			{ MessageTypes.Codes.F25, new MessageBuilderDetails(typeof(F25MessageBuilder), typeof(F25HeaderProvider)) },
			{ MessageTypes.Codes.F26, new MessageBuilderDetails(typeof(F26MessageBuilder), typeof(F26HeaderProvider)) },
			{ MessageTypes.Codes.F40, new MessageBuilderDetails(typeof(F40MessageBuilder), typeof(F40HeaderProvider)) },
			{ MessageTypes.Codes.F41, new MessageBuilderDetails(typeof(F41MessageBuilder), typeof(F41HeaderProvider)) },
			{ MessageTypes.Codes.F43, new MessageBuilderDetails(typeof(F43MessageBuilder), typeof(F43HeaderProvider)) },
			{ MessageTypes.Codes.F44, new MessageBuilderDetails(typeof(F44MessageBuilder), typeof(F44HeaderProvider)) },
			{ MessageTypes.Codes.F50, new MessageBuilderDetails(typeof(F50MessageBuilder), typeof(F50HeaderProvider)) },
			{ MessageTypes.Codes.F51, new MessageBuilderDetails(typeof(F51MessageBuilder), typeof(F51HeaderProvider)) },
			{ MessageTypes.Codes.A10, new MessageBuilderDetails(typeof(A10MessageBuilder), typeof(A10HeaderProvider)) },
			{ MessageTypes.Codes.A13, new MessageBuilderDetails(typeof(A13MessageBuilder), typeof(A13HeaderProvider)) },
			{ MessageTypes.Codes.A14, new MessageBuilderDetails(typeof(A14MessageBuilder), typeof(A14HeaderProvider)) },
			{ MessageTypes.Codes.A15, new MessageBuilderDetails(typeof(A15MessageBuilder), typeof(A15HeaderProvider)) },
			{ MessageTypes.Codes.A16, new MessageBuilderDetails(typeof(A16MessageBuilder), typeof(A16HeaderProvider)) },
			{ MessageTypes.Codes.A17, new MessageBuilderDetails(typeof(A17MessageBuilder), typeof(A17HeaderProvider)) },
			{ MessageTypes.Codes.A22, new MessageBuilderDetails(typeof(A22MessageBuilder), typeof(A22HeaderProvider)) },
			{ MessageTypes.Codes.A23, new MessageBuilderDetails(typeof(A23MessageBuilder), typeof(A23HeaderProvider)) },
			{ MessageTypes.Codes.A24, new MessageBuilderDetails(typeof(A24MessageBuilder), typeof(A24HeaderProvider)) },
			{ MessageTypes.Codes.A26, new MessageBuilderDetails(typeof(A26MessageBuilder), typeof(A26HeaderProvider)) },
			{ MessageTypes.Codes.A40, new MessageBuilderDetails(typeof(A40MessageBuilder), typeof(A40HeaderProvider)) },
			{ MessageTypes.Codes.A41, new MessageBuilderDetails(typeof(A41MessageBuilder), typeof(A41HeaderProvider)) },
			{ MessageTypes.Codes.A50, new MessageBuilderDetails(typeof(A50MessageBuilder), typeof(A50HeaderProvider)) },
			{ MessageTypes.Codes.A51, new MessageBuilderDetails(typeof(A51MessageBuilder), typeof(A51HeaderProvider)) },
			{ MessageTypes.Codes.Q04, new MessageBuilderDetails(typeof(Q04MessageBuilder), typeof(Q04HeaderProvider)) },
			{ MessageTypes.Codes.R02, new MessageBuilderDetails(typeof(R02MessageBuilder), typeof(R02HeaderProvider)) },
			{ MessageTypes.Codes.R03, new MessageBuilderDetails(typeof(R03MessageBuilder), typeof(R03HeaderProvider)) },
			{ MessageTypes.Codes.N06, new MessageBuilderDetails(typeof(N06MessageBuilder), typeof(N06HeaderProvider)) },
		}.ToImmutableDictionary();

		struct MessageBuilderDetails
		{
			public MessageBuilderDetails(Type messageBuilderType, Type messageHeaderProviderType)
			{
				MessageBuilderType = messageBuilderType;
				MessageHeaderProviderType = messageHeaderProviderType;
			}

			public Type MessageBuilderType;
			public Type MessageHeaderProviderType;
		}
	}
}
