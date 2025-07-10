// -----------------------------------------------------------------------
// <copyright file="ApplicationCodeListPartial.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Messaging.Integration
{
	partial class ApplicationCodeList
	{
		/// <summary>
		/// This list is used to opt out messages from JobDeclaration.Messages
		/// </summary>
		/// <returns></returns>
		public static string[] GetSystemApplicationCodes()
		{
			return new string[]
			{
				Codes.XMS,
				Codes.UniversalDataMessaging,
				Codes.NativeDataMessaging
			};
		}
	}
}
