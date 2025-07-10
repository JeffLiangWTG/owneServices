using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CH.Business;

public interface ISpecialMentions
{
	[ResourceStringData("Enterprise.Customs.CH.Business.ISpecialMentions|SpecialMentions", Caption = "Special Mentions")]
	ZString SpecialMentions { get; set; }
}
