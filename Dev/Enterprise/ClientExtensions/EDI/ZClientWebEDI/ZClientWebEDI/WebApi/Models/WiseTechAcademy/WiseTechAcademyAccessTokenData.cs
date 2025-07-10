using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Definitions.Authentication;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WiseTechAcademy
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct WiseTechAcademyAccessTokenData
	{
		public const string TokenType = AccessTokenTypes.WiseTechAcademyAccessTokenType;
		public string Token { get; set; }
		public Guid ContactPk { get; set; }
	}
}
