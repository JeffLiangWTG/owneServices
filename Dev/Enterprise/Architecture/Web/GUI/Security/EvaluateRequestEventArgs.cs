using System;
using System.Web;
using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security
{
	/// <summary>
	/// Defines the arguments used for the EvaluateRequest event.
	/// </summary>
	public class EvaluateRequestEventArgs : EventArgs
	{
		// Fields.
		readonly HttpApplication _application;
		bool _cancelEvaluation;
		readonly SecureWebPageSettings _settings;

		/// <summary>
		/// Gets the HttpApplication used to evaluate the request.
		/// </summary>
		public HttpApplication Application
		{
			get { return _application; }
		}

		/// <summary>
		/// Gets or sets a flag indicating whether or not to cancel the evaluation.
		/// </summary>
		public bool CancelEvaluation
		{
			get { return _cancelEvaluation; }
			set { _cancelEvaluation = value; }
		}

		/// <summary>
		/// Gets the SecureWebPageSettings used to evaluate the request.
		/// </summary>
		public SecureWebPageSettings Settings
		{
			get { return _settings; }
		}

		/// <summary>
		/// Creates an instance of EvaluateRequestEventArgs with an instance of SecureWebPageSettings.
		/// </summary>
		/// <param name="application">The HttpApplication for the current context.</param>
		/// <param name="settings">An instance of SecureWebPageSettings used for the evaluation of the request.</param>
		public EvaluateRequestEventArgs(HttpApplication application, SecureWebPageSettings settings)
			: base()
		{
			_application = application;
			_settings = settings;
		}
	}
}
