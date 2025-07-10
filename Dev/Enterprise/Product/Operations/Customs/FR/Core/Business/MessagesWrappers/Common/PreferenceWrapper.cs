using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class PreferenceWrapper : IPreference
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public PreferenceWrapper(CusEntryLine cusEntryLine)
		{
			this.entryLine = Argument.NotNull(cusEntryLine, "Entry line cannot be null");
		}
		public ZString CodePart1 => entryLine?.RandomLine?.JI_PrimaryPreference.SubstringSafe(0, 1) ?? ZString.Empty;

		public ZString CodePart2 => entryLine?.RandomLine?.JI_PrimaryPreference.SubstringSafe(1, 2) ?? ZString.Empty;

		readonly CusEntryLine entryLine;
	}
}
