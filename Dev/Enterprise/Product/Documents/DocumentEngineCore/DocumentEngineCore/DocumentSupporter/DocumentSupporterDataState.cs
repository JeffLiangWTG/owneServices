namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public class DocumentSupporterDataState
	{
		public DocumentSupporterDataState()
		{
			IsValid = true;
			ErrorMessage = "";
		}

		/// <summary>
		/// The state of the data for the IDocumentSupport object
		/// </summary>
		/// <param name="isValid"> If the data is not valid, then an error message will be displayed. </param>
		/// <param name="errorMessage"> The error message to display in a dialogue box if the data is not valid </param>
		public DocumentSupporterDataState(bool isValid, string errorMessage)
		{
			this.IsValid = isValid;
			this.ErrorMessage = errorMessage;
		}

		/// <summary>
		/// If not valid, then the command will not be run and the error message will be displayed.
		/// </summary>
		public bool IsValid;

		/// <summary>
		/// The error message to display if IsValid is false.
		/// </summary>
		public string ErrorMessage;
	}
}
