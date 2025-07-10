using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.ZArchitecture.Environment
{
	public class GuidArrayRegistryDataType : RegistryDataType<Guid[]>
	{
		public GuidArrayRegistryDataType()
			: this(Array.Empty<Guid>())
		{
		}

		public GuidArrayRegistryDataType(Guid[] defaultValue)
			: base(RegistryDataTypes.Codes.GuidArray, defaultValue)
		{
		}

		protected override bool ValuesAreEqualCore(Guid[] a, Guid[] b)
		{
			return a.Length == b.Length && a.SequenceEqual(b);
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override byte[] SerialiseCore(Guid[] value)
		{
			return Encoding.Unicode.GetBytes(GuidArrayToString(value));
		}

		protected override Guid[] DeserialiseCore(byte[] value)
		{
			return StringToGuidArray(Encoding.Unicode.GetString(value));
		}

		protected override Guid[] CloneValue(Guid[] value)
		{
			return (Guid[])value.Clone();
		}

		internal Guid[] StringToGuidArray(string value)
		{
			string[] sGuids = value.Split(',');
			List<Guid> guids = new List<Guid>();
			foreach (string sGuid in sGuids)
			{
				try
				{
					Guid newGuid = new Guid(sGuid);
					if (newGuid != Guid.Empty)
					{
						guids.Add(newGuid);
					}
				}
				catch (ArgumentNullException)
				{
				}
				catch (FormatException)
				{
				}
			}
			return guids.ToArray();
		}

		internal string GuidArrayToString(Guid[] guids)
		{
			StringBuilder guidList = new StringBuilder();
			foreach (Guid item in guids)
			{
				if (item != Guid.Empty)
				{
					if (guidList.Length > 0)
					{
						guidList.Append(",");
					}
					guidList.Append(item.ToString());
				}
			}
			return guidList.ToString();
		}
	}
}
