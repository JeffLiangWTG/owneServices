using CargoWise.ServiceManager.Next.Launcher.Filters;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.ServiceManager.Next.Launcher.Controllers;

[ApiController]
[DbAccessTokenAuthorization]
[Route($"api/{RunnerCode}")]
public class TokenController : ControllerBase
{
	const string RunnerCode = "token";

	[HttpPost]
	[Route("signCW")]
	public Task<SignCwTokenResponse> RunRequestAsync([FromBody] SignCwTokenRequest request, [FromServices] INextProcessRunnerPool nextProcessRunnerPool, CancellationToken cancellationToken)
	{
		return nextProcessRunnerPool.RunAsync(RunnerCode, ICommandSender.SignCwTokenHandler, request, cancellationToken);
	}

	[HttpPost]
	[Route("resetAccessToken")]
	public Task<ResetAccessTokenResponse> RunRequestAsync([FromBody] ResetAccessTokenRequest request, [FromServices] INextProcessRunnerPool nextProcessRunnerPool, CancellationToken cancellationToken)
	{
		return nextProcessRunnerPool.RunAsync(RunnerCode, ICommandSender.ResetAccessTokenHandler, request, cancellationToken);
	}

	[HttpPost]
	[Route("prepareNewCertificate")]
	public Task<PrepareNewCertificateResponse> RunRequestAsync([FromBody] PrepareNewCertificateRequest request, [FromServices] INextProcessRunnerPool nextProcessRunnerPool, CancellationToken cancellationToken)
	{
		return nextProcessRunnerPool.RunAsync(RunnerCode, ICommandSender.PrepareNewCertificateHandler, request, cancellationToken);
	}

	[HttpPost]
	[Route("setNewCertificateCredentials")]
	public Task<SetNewCertificateCredentialsResponse> RunRequestAsync([FromBody] SetNewCertificateCredentialsRequest request, [FromServices] INextProcessRunnerPool nextProcessRunnerPool, CancellationToken cancellationToken)
	{
		return nextProcessRunnerPool.RunAsync(RunnerCode, ICommandSender.SetNewCertificateCredentialsHandler, request, cancellationToken);
	}

	[HttpPost]
	[Route("setOperationId")]
	public Task<SetOperationIdResponse> RunRequestAsync([FromBody] SetOperationIdRequest request, [FromServices] INextProcessRunnerPool nextProcessRunnerPool, CancellationToken cancellationToken)
	{
		return nextProcessRunnerPool.RunAsync(RunnerCode, ICommandSender.SetOperationIdHandler, request, cancellationToken);
	}
}
