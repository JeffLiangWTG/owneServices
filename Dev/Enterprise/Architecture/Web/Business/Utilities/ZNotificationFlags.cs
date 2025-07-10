using System;
using System.Collections;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// Summary description for ZNotificationFlags.
	/// </summary>
	[Serializable]
	public class ZNotificationFlags : System.Runtime.Serialization.ISerializable
	{
		readonly BitArray Flags;

		public ZNotificationFlags()
		{
			Flags = new BitArray(3);
		}

#if NETFRAMEWORK
		public ZNotificationFlags(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : this()
		{
			DisplayErrors = info.GetBoolean("DisplayErrors");
			DisplayWarnings = info.GetBoolean("DisplayWarnings");
			DisplayMessageErrors = info.GetBoolean("DisplayMessageErrors");
		}
#endif

		public ZBool DisplayErrors
		{
			get { return Flags[0]; }
			set { Flags[0] = value; }
		}

		public ZBool DisplayWarnings
		{
			get { return Flags[1]; }
			set { Flags[1] = value; }
		}

		public ZBool DisplayMessageErrors
		{
			get { return Flags[2]; }
			set { Flags[2] = value; }
		}

		public ZBool DisplayAll
		{
			get { return DisplayErrors && DisplayMessageErrors && DisplayWarnings; }
			set { Flags.SetAll(value); }
		}

		public ZBool DisplayAny
		{
			get { return DisplayErrors || DisplayMessageErrors || DisplayWarnings; }
		}

		#region ISerializable Members

		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("DisplayErrors", DisplayErrors);
			info.AddValue("DisplayWarnings", DisplayWarnings);
			info.AddValue("DisplayMessageErrors", DisplayMessageErrors);
		}
		#endregion
	}
}
