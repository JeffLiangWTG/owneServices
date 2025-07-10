using System;
using System.Collections;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void ScanningFinishedEventHandler(object sender, ScanningFinishedEventArgs ea);

	/// <summary>
	/// After a single page has been scanned
	/// </summary>
	public class ScanningFinishedEventArgs : EventArgs, IEnumerable
	{
		string fErrorMessage;
		public string ErrorMessage
		{
			get { return fErrorMessage; }
			set { fErrorMessage = value; }
		}

		readonly ArrayList fFileDetails;

		public int FileDetailsCount
		{
			get { return fFileDetails.Count; }
		}

		public DocumentResult GetFileDetail(int index)
		{
			return (DocumentResult)fFileDetails[index];
		}

		public void AddFileDetail(DocumentResult resultToAdd)
		{
			fFileDetails.Add(resultToAdd);
		}

		public IEnumerator GetEnumerator()
		{
			return fFileDetails.GetEnumerator();
		}

		public ScanningFinishedEventArgs()
		{
			fFileDetails = new ArrayList();
		}
	}
}
