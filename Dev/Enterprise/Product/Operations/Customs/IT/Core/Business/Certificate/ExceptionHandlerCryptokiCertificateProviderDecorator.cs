using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CargoWise.Common;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public sealed class ExceptionHandlerCryptokiCertificateProviderDecorator : ICryptokiCertificateProvider
{
	public ExceptionHandlerCryptokiCertificateProviderDecorator(ICryptokiCertificateProvider cryptokiCertificateProvider)
	{
		this.cryptokiCertificateProvider = Argument.NotNull(cryptokiCertificateProvider, nameof(cryptokiCertificateProvider));
	}

	IReadOnlyList<CryptokiCertificate> ICryptokiCertificateProvider.GetCertificateList(string chipset)
	{
		try
		{
			return cryptokiCertificateProvider.GetCertificateList(chipset);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			HandleCannotFindLibraryException(ex, chipset);
			throw;
		}
	}

	CryptokiCertificate ICryptokiCertificateProvider.ReadCertificate(string chipset, byte[] serialNumber)
	{
		try
		{
			return cryptokiCertificateProvider.ReadCertificate(chipset, serialNumber);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			HandleCannotFindLibraryException(ex, chipset);
			throw;
		}
	}

	void HandleCannotFindLibraryException(Exception ex, string chipset)
	{
		if (IsIOCannotFindLibraryException() || IsCryptoCannotFindLibraryException())
		{
			throw new CryptographicException(GetNotFoundDriverExceptionMessage(chipset), ex);
		}

		bool IsIOCannotFindLibraryException()
			 => ex is IOException iOException
			 && IsCannotFindLibraryException(iOException);

		bool IsCryptoCannotFindLibraryException()
			 => ex is CryptographicException cryptoException
			 && IsCannotFindLibraryException(cryptoException);
	}

	string GetNotFoundDriverExceptionMessage(string chipsetCode)
	{
		var chipset = (Chipset)Enum.Parse(typeof(Chipset), chipsetCode);
		return Res.GetString("CE5A42A9-6D88-482A-AC8E-8E3DE0D233C0",
			"The system cannot find the drivers for your Certificate ({0}). Please install the drivers following the instructions of the manufacturer.",
			Pkcs11Utils.GetPkcs11LibraryDllName(chipset));
	}

	bool IsCannotFindLibraryException(Exception exception)
		=> exception.Message.Contains((NoResString)"Cannot find PKCS#11 library.");

	readonly ICryptokiCertificateProvider cryptokiCertificateProvider;
}
