USE [master]
GO

/****** Object:  LinkedServer [SSAS2016]    Script Date: 31/05/2017 9:46:04 AM ******/
EXEC master.dbo.sp_addlinkedserver @server = N'SSAS2016', @srvproduct=N'', @provider=N'MSOLAP', @datasrc=N'sydco-ssql-12b', @catalog=N'billing'
 /* For security reasons the linked server remote logins password is changed with ######## */
EXEC master.dbo.sp_addlinkedsrvlogin @rmtsrvname=N'SSAS2016',@useself=N'True',@locallogin=NULL,@rmtuser=NULL,@rmtpassword=NULL

GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'collation compatible', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'data access', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'dist', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'pub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'rpc', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'rpc out', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'sub', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'connect timeout', @optvalue=N'0'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'collation name', @optvalue=null
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'lazy schema validation', @optvalue=N'false'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'query timeout', @optvalue=N'21600'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'use remote collation', @optvalue=N'true'
GO

EXEC master.dbo.sp_serveroption @server=N'SSAS2016', @optname=N'remote proc transaction promotion', @optvalue=N'true'
GO


