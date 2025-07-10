using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ServiceManager.Host;

[CodeAlive("Http Error code comparison")]
enum HttpListenerErrorCodes
{
	ErrorAccessDenied = 5,
	HandleIsInvalid = 6,
	ErrorNetNameDeleted = 64,
	ErrorOperationAborted = 99,
	ErrorAlreadyExists = 183,
	ErrorNonexistentNetworkConnection = 1229,
}
