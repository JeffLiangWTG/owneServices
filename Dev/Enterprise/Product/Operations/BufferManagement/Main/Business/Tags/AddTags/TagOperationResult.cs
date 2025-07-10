using CargoWise.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class TagOperationResult : ITagOperationResult
	{
		internal static TagOperationResult Success(ITagLink link)
		{
			Argument.NotNull(link, "link");

			return new TagOperationResult { WasSuccessful = true, Link = link };
		}

		internal static TagOperationResult NoOperation(string message)
		{
			return new TagOperationResult { WasSuccessful = true, Message = message };
		}

		internal static TagOperationResult Failed(string message)
		{
			return new TagOperationResult { WasSuccessful = false, Message = message };
		}

		protected TagOperationResult()
		{
		}

		public bool WasSuccessful { get; protected set; }
		public string Message { get; protected set; }
		public ITagLink Link { get; protected set; }

		internal static string NoPermissionMessage { get { return Res.GetString("94c4b63c-7350-46d4-9034-c81fd28b9f0d", "You do not have permission to perform this action."); } }
		internal static string ObjectNotInDatabase { get { return Res.GetString("c05ea1a7-addc-46d1-bc24-f95b9e5a74ca", "Whilst you were working another user has deleted some information you are attempting to see."); } }
	}
}
