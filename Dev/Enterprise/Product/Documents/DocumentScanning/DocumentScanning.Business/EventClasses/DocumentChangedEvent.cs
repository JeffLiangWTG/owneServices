using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void DocumentChangedEventHandler(object sender, DocumentChangedEventArgs e);

	public class DocumentChangedEventArgs : EventArgs
	{
		public DocumentChangedEventArgs(Event change, string desc)
		{
			fDescription = desc;
			fChangeType = change;
		}

		public ZString Description
		{
			get { return fDescription; }
		}
		readonly ZString fDescription;

		public Event ChangeType
		{
			get { return fChangeType; }
		}
		readonly Event fChangeType;
	}
}
