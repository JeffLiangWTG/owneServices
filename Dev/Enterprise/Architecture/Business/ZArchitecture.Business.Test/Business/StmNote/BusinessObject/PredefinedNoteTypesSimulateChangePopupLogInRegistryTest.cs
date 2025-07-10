using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PredefinedNoteTypesSimulateChangePopupLogInRegistryTest : PredefinedNoteTypes
	{
		public new static PredefinedNoteTypesSimulateChangePopupLogInRegistryTest Instance
		{
			get { return (PredefinedNoteTypesSimulateChangePopupLogInRegistryTest)PredefinedNoteTypes.Instance; }
		}

		public static void Register()
		{
			OverrideNewDelegate(New);
		}

		public static void Unregister()
		{
			ResetNewDelegate();
		}

		static PredefinedNoteTypesSimulateChangePopupLogInRegistryTest New()
		{
			return new PredefinedNoteTypesSimulateChangePopupLogInRegistryTest();
		}

		public PredefinedNoteType PopupLogNote
		{
			get
			{
				if (popupLogNote == null)
				{
					popupLogNote = new PredefinedNoteType((NoResString)"PopupLogNote", StmNoteVisibility.PUB, IsUniqueInCollection, !IsReadOnlyAfterAdd, !IsTextOnly, !IsPopupLog, false);
				}
				return popupLogNote;
			}
		}
		PredefinedNoteType popupLogNote;
	}
}
