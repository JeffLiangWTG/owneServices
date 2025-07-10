using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public abstract class RequestWrapper
	{
		protected RequestWrapper(Request request)
		{
			this.request = request;
		}
		protected readonly Request request;

		public HeaderData Settings
		{
			get { return request.Settings; }
		}

		public abstract ValidateResult Validate();
	}

	public class UpdateRequest : RequestWrapper
	{
		public UpdateRequest(Request request)
			: base(request)
		{
		}

		public XElement Body
		{
			get { return request.EntitySets.FirstOrDefault(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override ValidateResult Validate()
		{
			var entitySetXml = request.EntitySets;

			var entitySetCount = entitySetXml == null ? 0 : entitySetXml.Count();

			if (entitySetCount == 0)
			{
				return new ValidateResult { IsSuccess = false, Message = "Body element must not be empty." };
			}

			if (entitySetCount > 1)
			{
				return new ValidateResult { IsSuccess = false, Message = "Only one entity set per Body element can be handled by this Service." };
			}

			return ValidateResult.Default;
		}
	}

	public class RetrieveRequest : RequestWrapper
	{
		public RetrieveRequest(Request request)
			: base(request)
		{
		}

		public XElement Body
		{
			get { return request.EntitySets.FirstOrDefault(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override ValidateResult Validate()
		{
			var entitySets = request.EntitySets;

			if (entitySets == null || !entitySets.Any())
			{
				return new ValidateResult { IsSuccess = false, Message = "Body element must not be empty." };
			}

			return ValidateResult.Default;
		}
	}

	public class ExportImportRequest : RequestWrapper
	{
		public ExportImportRequest(Request request)
			: base(request)
		{
		}

		public IEnumerable<XElement> EntitySets
		{
			get { return request.EntitySets; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override ValidateResult Validate()
		{
			if (request == null)
			{
				return new ValidateResult { IsSuccess = false, Message = "Request must not be empty." };
			}

			if (request.EntitySets == null)
			{
				return new ValidateResult { IsSuccess = false, Message = "Body element must not be empty." };
			}

			return ValidateResult.Default;
		}
	}

	public class Request
	{
		public HeaderData Settings { get; set; }
		public IEnumerable<XElement> EntitySets { get; set; }
	}

	public struct ValidateResult
	{
		public static ValidateResult Default
		{
			get
			{
				var result = new ValidateResult { IsSuccess = true, Message = string.Empty };
				return result;
			}
		}

		public bool IsSuccess { get; set; }
		public string Message { get; set; }
	}
}
