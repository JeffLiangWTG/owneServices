using Enterprise.DocumentVisualizer.Integration;
using Res = Enterprise.DocumentVisualizer.Business.Res;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public static class CommandResources
	{
		public static class Captions
		{
			public static string DeliverDocument => Res.GetString("be6019dd-910f-498c-ba2e-90a40ca7a4f9", "Deliver Document");
			public static string SendMessage => Res.GetString("ac37ec1e-66af-4f9b-b0b2-64c07132a453", "Send Message");
			public static string SendWithdrawal => Res.GetString("4c96d9ac-02bc-45c7-86ea-b3db29f8f8da", "Withdraw/Cancel Message");
			public static string ResetToOriginal => Res.GetString("3f0ee7d0-b5ee-43e6-b22f-2a3eb7f43da8", "Reset To Original");
			public static string ShowMacroEvaluator => Res.GetString("359ecf44-8c5b-415d-b12d-ed155de616bd", "Macro Evaluator");
			public static string ShowDocumentData => Res.GetString("99457a9b-65bb-4c05-849d-e9ef06cbc1c5", "Document Data");
			public static string ShowMessagingData => Res.GetString("82f50e9a-bec1-4925-a74d-2d9da3772e8f", "Messaging Data");
			public static string ShowOverriddenData => Res.GetString("32d1d1bf-ea78-44f3-97ee-674d129d406e", "Overridden Data");
			public static string ResetOverriddenData => Res.GetString("4086E265-D011-4883-8FDE-AA0311850A3F", "Reset");
			public static string SaveOverriddenData => Res.GetString("322c150f-6aad-4956-94d6-914487e7da15", "Save");
			public static string Exit => Res.GetString("87f8ff87-b5e1-4578-8a70-3987de799347", "Exit");

			public static string GetCaptionForCommand(string commandId)
			{
				switch (commandId)
				{
					case CommandIds.DeliverDocument:
						return DeliverDocument;

					case CommandIds.SendMessage:
						return SendMessage;

					case CommandIds.SendWithdrawal:
						return SendWithdrawal;

					case CommandIds.ResetToOriginal:
						return ResetToOriginal;

					case CommandIds.ShowMacroEvaluator:
						return ShowMacroEvaluator;

					case CommandIds.ShowDocumentData:
						return ShowDocumentData;

					case CommandIds.ShowMessagingData:
						return ShowMessagingData;

					case CommandIds.ShowOverriddenData:
						return ShowOverriddenData;

					case CommandIds.ResetOverriddenData:
						return ResetOverriddenData;

					case CommandIds.SaveOverriddenData:
						return SaveOverriddenData;

					case CommandIds.Exit:
						return Exit;
				}

				return string.Empty;
			}
		}
	}
}
