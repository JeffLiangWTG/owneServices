using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.Common.MessageBuilders
{
	public class BuilderResult : IBuilderResult
	{
		public BuilderResult(BusinessObject owner, IEnumerable<string> errors, AfterFullSuccessDelegate afterFullSuccessDelegate)
		{
			this.owner = owner;
			this.errors = errors;
			this.afterFullSuccessDelegate = afterFullSuccessDelegate;
		}
		readonly IEnumerable<string> errors;
		readonly BusinessObject owner;
		readonly AfterFullSuccessDelegate afterFullSuccessDelegate;

		public EDIMessage Message { get; set; }

		public BusinessObject Owner
		{
			get { return owner; }
		}

		public AfterFullSuccessDelegate AfterFullSuccessDelegate
		{
			get { return afterFullSuccessDelegate; }
		}

		public void AfterFullSuccess()
		{
			if (AfterFullSuccessDelegate != null)
			{
				AfterFullSuccessDelegate(this);
			}
		}

		public string[] Errors
		{
			get { return ErrorsCore(); }
		}

		protected virtual string[] ErrorsCore()
		{
			List<string> result = new List<string>();
			foreach (string error in errors)
			{
				result.Add(error);
			}
			return result.ToArray();
		}
	}
}
