using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionAcceptabilityBandLookups : ZLookups
	{
		public BoardSectionAcceptabilityBandLookups(BoardSectionAcceptabilityBand parent)
			: base(parent)
		{
		}

		new BoardSectionAcceptabilityBand Parent => (BoardSectionAcceptabilityBand)base.Parent;

		#region AvailableAcceptabilityBands

		public BMComponentAcceptabilityBandCollection AvailableAcceptabilityBands => new BMComponentAcceptabilityBandCollection(Factory);

		#endregion

		#region Visualization Options

		public CodeDescriptionPairList FiltersByReleaseGroupOverrideOptions
		{
			get
			{
				var defaultOption = GetDefaultOption(Parent.AcceptabilityBandPK.IsEmpty ? null : Parent.AcceptabilityBand.BAB_FiltersByReleaseGroupInfo);
				var yesOption = new CodeDescriptionPair(YesOptionCode, Res.GetString("547991d0-07df-4944-ad4e-0ce9ba214afd", "Filter results by section release group"));
				var noOption = new CodeDescriptionPair(NoOptionCode, Res.GetString("c75b5d93-b549-477b-a94a-8b4d4c837da3", "Do not filter results by section release group"));

				return new CodeDescriptionPairList { defaultOption, yesOption, noOption };
			}
		}

		public CodeDescriptionPairList FiltersBySectionOverrideOptions
		{
			get
			{
				var defaultOption = GetDefaultOption(Parent.AcceptabilityBandPK.IsEmpty ? null : Parent.AcceptabilityBand.BAB_FiltersBySectionInfo);
				var yesOption = new CodeDescriptionPair(YesOptionCode, Res.GetString("5f5f8ccb-75c7-4747-9c39-6eb62c365c62", "Filter results by board section filters"));
				var noOption = new CodeDescriptionPair(NoOptionCode, Res.GetString("465615b2-0f0f-4732-b074-0b99f178c925", "Do not filter results by board section filters"));

				return new CodeDescriptionPairList { defaultOption, yesOption, noOption };
			}
		}

		ICodeDescription GetDefaultOption(IZPropertyInfo bandDefaultSetting)
		{
			var description = Res.GetString("04164d86-ede6-45cc-b3cb-b88380ff09f3", "Use the value specified on the Acceptability Band configuration form");

			if (!Parent.AcceptabilityBandPK.IsEmpty && bandDefaultSetting != null)
			{
				description += " ";
				description += Res.GetString("ab284659-5ccd-42e3-b13c-f933df7c2394", "[{0}]", (ZBool)(bandDefaultSetting.Value) ? YesOptionCode : NoOptionCode);
			}

			return new CodeDescriptionPair(DefaultOptionCode, description);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codes should not be translated")]
		public const string DefaultOptionCode = "Default";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codes should not be translated")]
		public const string YesOptionCode = "Yes";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Codes should not be translated")]
		public const string NoOptionCode = "No";

		#endregion

		#region Show On

		public CodeDescriptionPairList ShowOnOptions => new AcceptabilityBandShowOnOptions();

		#endregion
	}
}
