using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class ZBoolDescriptionPair
	{
		public ZBoolDescriptionPair(string description, ZBool value) : this(ZGuid.Empty, description, value)
		{
		}

		public ZBoolDescriptionPair(ZGuid pK, string description, ZBool value)
		{
			fDescription = description;
			fValue = value;
			fPK = pK;
		}

		#region Change Event

		public event EventHandler OnChanged;

		#endregion

		#region Description

		public ZString Description
		{
			get { return fDescription; }
			set
			{
				fDescription = value;
				if (OnChanged != null)
				{
					OnChanged(this, EventArgs.Empty);
				}
			}
		}

		ZString fDescription;

		#endregion

		#region Value

		public ZBool Value
		{
			get { return fValue; }
			set
			{
				fValue = value;
				if (OnChanged != null)
				{
					OnChanged(this, EventArgs.Empty);
				}
			}
		}

		ZBool fValue;

		#endregion

		#region PK

		public ZGuid PK
		{
			get { return fPK; }
			set
			{
				fPK = value;
				if (OnChanged != null)
				{
					OnChanged(this, EventArgs.Empty);
				}
			}
		}

		ZGuid fPK;

		#endregion
	}
}
