using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class BulkUpdateMode : NonPersistentBusinessObject, System.Runtime.Serialization.ISerializable, IObsoleteValidation, IBindableBooleanItem
	{
		public BulkUpdateMode(string displayName, int recordsAffected, string modeCode = "")
		{
			DisplayName = displayName;
			RecordsAffected = recordsAffected;
			Selected = false;
			ModeCode = modeCode;
		}

		public string ModeCode { get; }
		public string DisplayName { get; }
		public int RecordsAffected { get; set; }
		public bool Selected { get; set; }

		public ZBool BoolValue
		{
			get
			{
				return Selected;
			}
			set
			{
				Selected = value;
				BoolValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BoolValueInfo
		{
			get { return GetZPropertyInfo(nameof(BoolValue)); }
		}

		public ZString Text
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", DisplayName, RecordsAffected); }
		}

		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("DisplayName", DisplayName);
			info.AddValue("RecordsAffected", RecordsAffected);
		}
	}

	public static class BulkUpdateModeCodes
	{
		public const string AllContacts = "ALL";
		public const string SelectedContacts = "SEL";
	}
}