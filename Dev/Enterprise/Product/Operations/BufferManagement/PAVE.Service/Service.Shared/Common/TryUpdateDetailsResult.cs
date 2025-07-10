using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared.Common.Dtos;

namespace Enterprise.BufferManagement.Service.Common
{
	public class TryUpdateDetailsResult
	{
		public TryUpdateDetailsResult(ContentWithHashDto detailsResponse)
		{
			DetailsResponse = detailsResponse;
		}

		public TryUpdateDetailsResult(IEnumerable<string> validationErrorMessages)
		{
			Error = new PaveError(PaveErrorCode.ValidationError, validationErrorMessages);
		}

		public ContentWithHashDto DetailsResponse { get; }

		public PaveError Error { get; }

		public bool Success => Error == null;
	}
}
