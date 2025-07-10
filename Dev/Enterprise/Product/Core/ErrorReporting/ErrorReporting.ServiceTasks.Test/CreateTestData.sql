DELETE [StmErrorReport];

INSERT INTO [StmErrorReport]
([QER_PK], [QER_ReportXml], [QER_TransmitStatus], [QER_SystemCreateTimeUtc], [QER_SystemCreateUser], [QER_SystemLastEditTimeUtc], [QER_SystemLastEditUser])
VALUES

-- Fresh report
('AA49A965-6DF2-4356-908B-8E12EB256C3C', '<EDI_Exception_Report><ErrorReportID>TEST1</ErrorReportID></EDI_Exception_Report>', 'QUE', SYSUTCDATETIME(), 'E', SYSUTCDATETIME(), 'E'),

-- Slighly old
('3F0F71CB-2B67-490F-880A-CBDFCB9146BF', '<EDI_Exception_Report><ErrorReportID>TEST2</ErrorReportID></EDI_Exception_Report>', 'QUE', DATEADD(MINUTE, -35, SYSUTCDATETIME()), 'E', DATEADD(MINUTE, -35, SYSUTCDATETIME()), 'E'),
('EFC3F319-109C-4D65-B48E-7C783D4FB1B3', '<EDI_Exception_Report><ErrorReportID>TEST3</ErrorReportID></EDI_Exception_Report>', 'SNT', DATEADD(MINUTE, -35, SYSUTCDATETIME()), 'E', DATEADD(MINUTE, -35, SYSUTCDATETIME()), 'E'),

-- 7 days old, plus one minute
('469A28CE-B612-4D04-A981-BBE4434B4C3E', '<EDI_Exception_Report><ErrorReportID>TEST4</ErrorReportID></EDI_Exception_Report>', 'QUE', DATEADD(MINUTE, +1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E', DATEADD(MINUTE, +1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E'),
('FD429F7D-54A8-4CAF-BCFA-853C36ADE13A', '<EDI_Exception_Report><ErrorReportID>TEST5</ErrorReportID></EDI_Exception_Report>', 'SNT', DATEADD(MINUTE, +1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E', DATEADD(MINUTE, +1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E'),

-- 7 days old, minus one minute
('89FDF4A7-D9A9-47A0-AF5E-61C659A8FDAF', '<EDI_Exception_Report><ErrorReportID>TEST6</ErrorReportID></EDI_Exception_Report>', 'QUE', DATEADD(MINUTE, -1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E', DATEADD(MINUTE, -1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E'),
('AB08D02F-D037-4490-A173-4C292B7C5FD1', '<EDI_Exception_Report><ErrorReportID>TEST7</ErrorReportID></EDI_Exception_Report> />', 'SNT', DATEADD(MINUTE, -1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E', DATEADD(MINUTE, -1, DATEADD(DAY, -7, SYSUTCDATETIME())), 'E'),

-- 4 weeks old
('44F0E8B6-61EB-4384-9002-76CB582E0BC9', '<EDI_Exception_Report><ErrorReportID>TEST8</ErrorReportID></EDI_Exception_Report>', 'QUE', DATEADD(DAY, -28, SYSUTCDATETIME()), 'E', DATEADD(DAY, -28, SYSUTCDATETIME()), 'E'),
('25FE5DF2-8982-49A4-B8DA-4B8E3B25E961', '<EDI_Exception_Report><ErrorReportID>TEST9</ErrorReportID></EDI_Exception_Report>', 'SNT', DATEADD(DAY, -28, SYSUTCDATETIME()), 'E', DATEADD(DAY, -28, SYSUTCDATETIME()), 'E');
