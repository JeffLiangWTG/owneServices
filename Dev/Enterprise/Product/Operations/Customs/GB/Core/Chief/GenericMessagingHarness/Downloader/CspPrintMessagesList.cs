using System.Collections.Generic;

namespace Enterprise.Customs.GB.Chief.CspPuller
{
	/// <summary>
	/// Simple structure to hold a batch of received Edi Print messages and their batch ID.  Given back to us by Destin8 when we call getAvailableEdifactPrints()
	/// </summary>

	public class CspPrintMessagesList
	{
		int batchId;
		readonly List<string> messages;
		public int BatchId
		{
			get { return this.batchId; }
			set { this.batchId = value; }
		}

		public CspPrintMessagesList(int batchId)
		{
			this.batchId = batchId;
			messages = new List<string>();
		}

		public List<string> Messages
		{
			get { return this.messages; }
		}

		public string AddMessage(string message)
		{
			this.messages.Add(message);
			return message;
		}

		ErrorStruct error;
		public ErrorStruct Error
		{
			get { return this.error; }
			set { this.error = value; }
		}

		public class ErrorStruct
		{
			public string ErrorText;
			public int ErrorNumber;
		}
	}
}
